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
using Microsoft.AspNetCore.Http;

namespace MilkChillar.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetTenantIdFromToken()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type.Equals("tenant_id", StringComparison.OrdinalIgnoreCase));

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                throw new UnauthorizedAccessException("Tenant ID not found in token claims.");

            if (!int.TryParse(claim.Value, out var tenantId))
                throw new UnauthorizedAccessException("Invalid Tenant ID in token.");

            return tenantId;
        }



        public async Task<int> CreateMainAccountAsync(CreateMainAccountRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            var nextCode = await GenerateNextMainAccountCode(tenantId, request.FinancialStatementComponent);

            var mainAccount = new MainAccount
            {
                TenantId = tenantId,
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
            int tenantId = GetTenantIdFromToken();

            var mainAccount = await _context.MainAccounts
                .FirstOrDefaultAsync(m => m.MainAccountId == request.MainAccountId && m.TenantId == tenantId);

            if (mainAccount == null) throw new Exception("Main account not found.");

            var nextCode = await GenerateNextSubAccountCode(tenantId, request.MainAccountId);

            var subAccount = new SubAccount
            {
                TenantId = tenantId,
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
            int tenantId = GetTenantIdFromToken();

            var subAccount = await _context.SubAccounts
                .Include(sa => sa.MainAccount)
                .FirstOrDefaultAsync(sa => sa.SubAccountId == request.SubAccountId && sa.TenantId == request.TenantId);

            if (subAccount == null) throw new Exception("Sub account not found.");

            var nextCode = await GenerateNextAccountCode(request.TenantId, request.SubAccountId);

            var fullCode = $"{subAccount.MainAccount.MainAccountCode}-{subAccount.SubAccountCode}-{nextCode}";

            var account = new Account
            {
                TenantId = tenantId,
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
        private async Task<string> GenerateNextMainAccountCode(int _, string component)
        {
            int tenantId= GetTenantIdFromToken();
            var (baseCode, rangeStart, rangeEnd) = component.ToLower() switch
            {
                "currentassets" => (100, 100, 199),
                "noncurrentassets" => (100, 100, 199),
                "currentliabilities" => (200, 200, 299),
                "noncurrentliabilities" => (200, 200, 299),
                "capitalandreserves" => (300, 300, 399),
                "revenue" => (400, 400, 499),
                "costofsales" => (500, 500, 599),
                "operatingexpenses" => (600, 600, 699),
                "financialexpenses" => (600, 600, 699),
                _ => (999, 999, 999)
            };

            // Get the maximum code in the range
            var maxCodeQuery = await _context.MainAccounts
                .Where(m => m.TenantId == tenantId)
                .Select(m => m.MainAccountCode)
                .Where(code => code != null)
                .ToListAsync();

            int? maxCode = maxCodeQuery
                .Where(code => int.TryParse(code, out int codeInt) &&
                              codeInt >= rangeStart &&
                              codeInt <= rangeEnd)
                .Select(code => int.Parse(code))
                .Cast<int?>()
                .Max();

            int nextCode = maxCode.HasValue ? maxCode.Value + 10 : baseCode;

            // Safety check
            if (nextCode > rangeEnd)
            {
                throw new InvalidOperationException($"No available codes in range {rangeStart}-{rangeEnd} for component {component}");
            }

            return nextCode.ToString();
        }


        // Generate next SubAccountCode per tenant+mainAccount
        private async Task<string> GenerateNextSubAccountCode(int _, int mainAccountId)
        {
            int tenantId = GetTenantIdFromToken();
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
        private async Task<string> GenerateNextAccountCode(int _, int subAccountId)
        {
            int tenantId = GetTenantIdFromToken();

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
            var newCode = baseCode + next.ToString("D3"); // ← change from D2 to D3

            return newCode;
        }


        public async Task<List<MainAccountDto>> GetMainAccountsAsync(int _)//parameter just for compatibility as updated in hurry
        {
            int tenantId = GetTenantIdFromToken();
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


        public async Task<List<SubAccountDto>> GetSubAccountsAsync(int _, int mainAccountId)
        {

            int tenantId = GetTenantIdFromToken();
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

        public async Task<List<SubAccountDto>> GetSubAccountsByMainAccountCodeAsync(string mainAccountCode)
        {
            int tenantId = GetTenantIdFromToken();

            return await _context.SubAccounts
                .Where(s => s.TenantId == tenantId && s.SubAccountCode.StartsWith(mainAccountCode))
                .Select(s => new SubAccountDto
                {
                    SubAccountId = s.SubAccountId,
                    Name = s.Name,
                    SubAccountCode = s.SubAccountCode,
                    MainAccountId = s.MainAccountId
                })
                .ToListAsync();
        }


        public async Task<List<AccountDto>> GetAccountsAsync(int _, int subAccountId)
        {

            int tenantId = GetTenantIdFromToken();
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

        public async Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(int _)
        {
            int tenantId = GetTenantIdFromToken();
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

        public async Task<List<AccountSearchDto>> SearchAccountsAsync(string query, string? mainAccountCode = null)
        {
            int tenantId = GetTenantIdFromToken();

            var accountsQuery = _context.VwAccountSearch
                .Where(x => x.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(mainAccountCode))
            {
                accountsQuery = accountsQuery
                    .Where(x => x.AccountCode.StartsWith(mainAccountCode));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                string lowered = query.ToLower();
                accountsQuery = accountsQuery
                    .Where(x => x.AccountCode.ToLower().Contains(lowered) ||
                                x.AccountName.ToLower().Contains(lowered));
            }

            return await accountsQuery
                .OrderBy(x => x.AccountCode)
                .Take(15)
                .Select(x => new AccountSearchDto
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    Name = x.AccountName,
                    Balance = x.Balance
                })
                .ToListAsync();
        }

        public async Task<List<AccountSearchDto>> GetAccountsByCodePrefixAsync(string codePrefix)
        {
            int tenantId = GetTenantIdFromToken();

            if (string.IsNullOrWhiteSpace(codePrefix))
            {
                throw new ArgumentException("Code prefix cannot be null or empty", nameof(codePrefix));
            }

            // Ensure we're working with exactly 3 digits
            string prefix = codePrefix.Trim();
            if (prefix.Length != 3)
            {
                throw new ArgumentException("Code prefix must be exactly 3 digits", nameof(codePrefix));
            }

            return await _context.VwAccountSearch
                .Where(x => x.TenantId == tenantId &&
                           x.AccountCode.StartsWith(prefix))
                .OrderBy(x => x.AccountCode)
                .Select(x => new AccountSearchDto
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    Name = x.AccountName,
                    Balance = x.Balance
                })
                .ToListAsync();
        }


    }
}
