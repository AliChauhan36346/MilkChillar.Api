using MilkChillar.Application;
using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for account balance reports
    /// </summary>
    public class AccountBalanceReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountBalanceReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedAccountBalancesDto> GetAccountBalancesAsync(
            string accountType,
            int tenantId,
            int pageNumber = 1,
            int pageSize = 25)
        {
            var (prefix, isSupplier) = AccountCodeHelper.GetAccountCodePrefix(accountType);

            var query = _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith(prefix));

            var totalCount = await query.CountAsync();

            var balances = await query
                .OrderByDescending(ab => Math.Abs(ab.DebitTotal - ab.CreditTotal))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ab => new AccountBalanceDetailDto
                {
                    AccountId = ab.AccountId,
                    AccountCode = ab.Account.AccountCode,
                    AccountName = ab.Account.Name,
                    AccountType = accountType,
                    DebitTotal = ab.DebitTotal,
                    CreditTotal = ab.CreditTotal,
                    Balance = AccountCodeHelper.CalculateBalance(ab.DebitTotal, ab.CreditTotal, isSupplier),
                    LastUpdated = ab.LastUpdated
                })
                .ToListAsync();

            var summary = await GetAccountBalanceSummaryAsync(accountType, tenantId);

            return new PagedAccountBalancesDto
            {
                Balances = balances,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Summary = summary
            };
        }

        public async Task<AccountBalanceSummaryDto> GetAccountBalanceSummaryAsync(
            string accountType,
            int tenantId)
        {
            var (prefix, isSupplier) = AccountCodeHelper.GetAccountCodePrefix(accountType);

            var balances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith(prefix))
                .ToListAsync();

            var totalDebit = balances.Sum(ab => ab.DebitTotal);
            var totalCredit = balances.Sum(ab => ab.CreditTotal);
            var netBalance = AccountCodeHelper.CalculateBalance(totalDebit, totalCredit, isSupplier);

            return new AccountBalanceSummaryDto
            {
                AccountType = accountType,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                NetBalance = netBalance,
                AccountCount = balances.Count
            };
        }
    }
}
