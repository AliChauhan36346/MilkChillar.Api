using MilkChillar.Application;
using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.Parameters;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for dashboard-related reports
    /// </summary>
    public class DashboardReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public DashboardReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int tenantId)
        {
            var today = DateTime.SpecifyKind(DateTime.Today.Date, DateTimeKind.Utc);

            // Get Cash and Bank Balances
            var cashBalance = await GetAccountTypeBalanceAsync(tenantId, "110");
            var bankBalance = await GetAccountTypeBalanceAsync(tenantId, "120");

            // Get Today's Cash and Bank Changes
            var todayCashChange = await GetTodayAccountChangeAsync(tenantId, "110", today);
            var todayBankChange = await GetTodayAccountChangeAsync(tenantId, "120", today);

            // Get Pending Payments (Suppliers)
            var (pendingPayments, pendingPaymentsCount) = await GetPendingPaymentsAsync(tenantId);

            // Get Due Receipts (Buyers)
            var (dueReceipts, dueReceiptsCount) = await GetDueReceiptsAsync(tenantId);

            return new AdminDashboardStatsDto
            {
                CashBalance = cashBalance,
                BankBalance = bankBalance,
                PendingPayments = pendingPayments,
                PendingPaymentsCount = pendingPaymentsCount,
                DueReceipts = dueReceipts,
                DueReceiptsCount = dueReceiptsCount,
                TodayCashChange = todayCashChange,
                TodayBankChange = todayBankChange
            };
        }

        private async Task<decimal> GetAccountTypeBalanceAsync(int tenantId, string accountCodePrefix)
        {
            return await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId && ab.Account.AccountCode.StartsWith(accountCodePrefix))
                .Select(ab => ab.DebitTotal - ab.CreditTotal)
                .FirstOrDefaultAsync();
        }

        private async Task<decimal> GetTodayAccountChangeAsync(int tenantId, string accountCodePrefix, DateTime today)
        {
            var accountIds = await _dbContext.Accounts
                .Where(a => a.TenantId == tenantId && a.AccountCode.StartsWith(accountCodePrefix))
                .Select(a => a.AccountId)
                .ToListAsync();

            var debits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              accountIds.Contains(jel.AccountId) &&
                              jel.Debit > 0)
                .SumAsync(jel => (decimal?)jel.Debit) ?? 0;

            var credits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              accountIds.Contains(jel.AccountId) &&
                              jel.Credit > 0)
                .SumAsync(jel => (decimal?)jel.Credit) ?? 0;

            return debits - credits;
        }

        private async Task<(decimal Total, int Count)> GetPendingPaymentsAsync(int tenantId)
        {
            var balances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith("200") &&
                             ab.CreditTotal > ab.DebitTotal)
                .ToListAsync();

            var total = balances.Sum(ab => ab.CreditTotal - ab.DebitTotal);
            return (total, balances.Count);
        }

        private async Task<(decimal Total, int Count)> GetDueReceiptsAsync(int tenantId)
        {
            var balances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith("100") &&
                             ab.DebitTotal > ab.CreditTotal)
                .ToListAsync();

            var total = balances.Sum(ab => ab.DebitTotal - ab.CreditTotal);
            return (total, balances.Count);
        }
    }
}
