using MilkChillar.Application.DTOs.Accounts;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateMainAccountAsync(CreateMainAccountRequest request)
        {
            var nextCode = await GenerateNextMainAccountCode(request.TenantId, request.FinancialStatementComponent);

            var mainAccount = new MainAccount
            {
                TenantId = request.TenantId,
                Name = request.Name,
                FinancialStatementComponent = request.FinancialStatementComponent,
                MainAccountCode = nextCode
            };

            _context.MainAccounts.Add(mainAccount);
            await _context.SaveChangesAsync();
            return mainAccount.MainAccountId;
        }

        public async Task<int> CreateSubAccountAsync(CreateSubAccountRequest request)
        {
            var mainAccount = await _context.MainAccounts
                .FirstOrDefaultAsync(m => m.MainAccountId == request.MainAccountId && m.TenantId == request.TenantId);

            if (mainAccount == null) throw new Exception("Main account not found.");

            var nextCode = await GenerateNextSubAccountCode(request.TenantId, request.MainAccountId);

            var subAccount = new SubAccount
            {
                TenantId = request.TenantId,
                MainAccountId = request.MainAccountId,
                Name = request.Name,
                SubAccountCode = nextCode
            };

            _context.SubAccounts.Add(subAccount);
            await _context.SaveChangesAsync();
            return subAccount.SubAccountId;
        }

        public async Task<int> CreateAccountAsync(CreateAccountRequest request)
        {
            var subAccount = await _context.SubAccounts
                .Include(sa => sa.MainAccount)
                .FirstOrDefaultAsync(sa => sa.SubAccountId == request.SubAccountId && sa.TenantId == request.TenantId);

            if (subAccount == null) throw new Exception("Sub account not found.");

            var nextCode = await GenerateNextAccountCode(request.TenantId, request.SubAccountId);

            var fullCode = $"{subAccount.MainAccount.MainAccountCode}-{subAccount.SubAccountCode}-{nextCode}";

            var account = new Account
            {
                TenantId = request.TenantId,
                SubAccountId = request.SubAccountId,
                Name = request.Name,
                AccountCode = nextCode,
                FullCode = fullCode
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account.AccountId;
        }

        // 🔢 Helper methods
        private async Task<string> GenerateNextMainAccountCode(int tenantId, string component)
        {
            var prefix = component.ToLower() switch
            {
                "assets" => "1",
                "liabilities" => "2",
                "equity" => "3",
                "revenue" => "4",
                "expenses" => "5",
                _ => "9"
            };

            var maxCode = await _context.MainAccounts
                .Where(m => m.TenantId == tenantId && m.MainAccountCode.StartsWith(prefix))
                .Select(m => (int?)Convert.ToInt32(m.MainAccountCode))
                .MaxAsync() ?? int.Parse(prefix + "00");

            return (maxCode + 1).ToString("D3");
        }


        // Generate next SubAccountCode per tenant+mainAccount
        private async Task<string> GenerateNextSubAccountCode(int tenantId, int mainAccountId)
        {
            var mainAccount = await _context.MainAccounts.FindAsync(mainAccountId);
            if (mainAccount == null)
                throw new Exception("Main account not found");

            var mainCode = mainAccount.MainAccountCode;

            var maxSub = await _context.SubAccounts
                .Where(sa => sa.TenantId == tenantId && sa.MainAccountId == mainAccountId)
                .Select(sa => (int?)Convert.ToInt32(sa.SubAccountCode.Substring(mainCode.Length)))
                .MaxAsync() ?? 0;

            var next = maxSub + 1;
            return mainCode + next.ToString("D2");
        }


        // Generate next AccountCode per tenant+subAccount
        private async Task<string> GenerateNextAccountCode(int tenantId, int subAccountId)
        {
            var subAccount = await _context.SubAccounts
                .Include(sa => sa.MainAccount)
                .FirstOrDefaultAsync(sa => sa.SubAccountId == subAccountId && sa.TenantId == tenantId);

            if (subAccount == null)
                throw new Exception("Sub account not found");

            var baseCode = subAccount.SubAccountCode;

            var maxAcc = await _context.Accounts
                .Where(a => a.TenantId == tenantId && a.SubAccountId == subAccountId)
                .Select(a => (int?)Convert.ToInt32(a.AccountCode.Substring(baseCode.Length)))
                .MaxAsync() ?? 0;

            var next = maxAcc + 1;
            var newCode = baseCode + next.ToString("D2");

            return newCode;
        }

        public async Task<List<MainAccountDto>> GetMainAccountsAsync(int tenantId)
        {
            return await _context.MainAccounts
                .Where(m => m.TenantId == tenantId)
                .Select(m => new MainAccountDto
                {
                    MainAccountId = m.MainAccountId,
                    Name = m.Name,
                    MainAccountCode = m.MainAccountCode,
                    FinancialStatementComponent = m.FinancialStatementComponent
                })
                .ToListAsync();
        }

        public async Task<List<SubAccountDto>> GetSubAccountsAsync(int tenantId, int mainAccountId)
        {
            return await _context.SubAccounts
                .Where(s => s.TenantId == tenantId && s.MainAccountId == mainAccountId)
                .Select(s => new SubAccountDto
                {
                    SubAccountId = s.SubAccountId,
                    Name = s.Name,
                    SubAccountCode = s.SubAccountCode,
                    MainAccountId = s.MainAccountId
                })
                .ToListAsync();
        }

        public async Task<List<AccountDto>> GetAccountsAsync(int tenantId, int subAccountId)
        {
            return await _context.Accounts
                .Where(a => a.TenantId == tenantId && a.SubAccountId == subAccountId)
                .Select(a => new AccountDto
                {
                    AccountId = a.AccountId,
                    Name = a.Name,
                    AccountCode = a.AccountCode,
                    FullCode = a.FullCode,
                    SubAccountId = a.SubAccountId
                })
                .ToListAsync();
        }

        public async Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(int tenantId)
        {
            var mainAccounts = await _context.MainAccounts
                .Where(m => m.TenantId == tenantId)
                .Include(m => m.SubAccounts)
                    .ThenInclude(sa => sa.Accounts)
                .ToListAsync();

            var result = mainAccounts.Select(m => new ChartOfAccountDto
            {
                MainAccountId = m.MainAccountId,
                MainAccountCode = m.MainAccountCode,
                Name = m.Name,
                FinancialStatementComponent = m.FinancialStatementComponent,
                SubAccounts = m.SubAccounts.Select(sa => new SubAccountNodeDto
                {
                    SubAccountId = sa.SubAccountId,
                    SubAccountCode = sa.SubAccountCode,
                    Name = sa.Name,
                    Accounts = sa.Accounts.Select(a => new AccountNodeDto
                    {
                        AccountId = a.AccountId,
                        AccountCode = a.AccountCode,
                        FullCode = a.FullCode,
                        Name = a.Name
                    }).ToList()
                }).ToList()
            }).ToList();

            return result;
        }


    }
}
