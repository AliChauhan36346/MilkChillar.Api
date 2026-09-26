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

            // Operational Metrics for Today
            var todayDateOnly = DateOnly.FromDateTime(today);
            var todayPurchases = await _dbContext.Purchases
                .Where(p => p.TenantId == tenantId && p.Date == todayDateOnly)
                .Select(p => new { p.GrossLiters, p.Rate })
                .ToListAsync();
            decimal todayPurchaseLiters = todayPurchases.Sum(p => p.GrossLiters);
            decimal todayPurchaseAmount = todayPurchases.Sum(p => p.GrossLiters * p.Rate);

            var todaySales = await _dbContext.Sales
                .Where(s => s.TenantId == tenantId && s.Date == todayDateOnly)
                .Select(s => new { s.NetLiters, s.Rate })
                .ToListAsync();
            decimal todaySalesLiters = todaySales.Sum(s => s.NetLiters);
            decimal todaySalesAmount = todaySales.Sum(s => s.NetLiters * s.Rate);

            // Last 6 Months Financial Trends (Live Real P&L)
            var sixMonthsAgo = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
            var journalData = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate >= sixMonthsAgo)
                .Select(jel => new
                {
                    jel.JournalEntry.EntryDate,
                    jel.Debit,
                    jel.Credit,
                    MainCode = jel.Account.SubAccount != null && jel.Account.SubAccount.MainAccount != null
                        ? jel.Account.SubAccount.MainAccount.MainAccountCode
                        : null
                })
                .ToListAsync();

            var monthlyTrends = new List<MonthlyFinancialTrendDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonthDate = today.AddMonths(-i);
                var yr = targetMonthDate.Year;
                var mo = targetMonthDate.Month;

                var monthLines = journalData.Where(x => x.EntryDate.Year == yr && x.EntryDate.Month == mo).ToList();
                decimal rev = monthLines.Where(x => x.MainCode != null && x.MainCode.StartsWith("4"))
                                        .Sum(x => x.Credit - x.Debit);
                decimal exp = monthLines.Where(x => x.MainCode != null && (x.MainCode.StartsWith("5") || x.MainCode.StartsWith("6") || x.MainCode.StartsWith("7")))
                                        .Sum(x => x.Debit - x.Credit);

                monthlyTrends.Add(new MonthlyFinancialTrendDto
                {
                    MonthLabel = targetMonthDate.ToString("MMM"),
                    Year = yr,
                    Month = mo,
                    Revenue = Math.Round(Math.Max(0, rev), 2),
                    Expense = Math.Round(Math.Max(0, exp), 2)
                });
            }

            // Recent 5 Transactions (Live Roznamcha Stream)
            var recentLines = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                .Where(jel => jel.JournalEntry.TenantId == tenantId && (jel.Debit > 0 || jel.Credit > 0))
                .OrderByDescending(jel => jel.JournalEntry.EntryDate)
                .ThenByDescending(jel => jel.JournalLineId)
                .Take(5)
                .Select(jel => new RecentTransactionDto
                {
                    JournalEntryId = jel.JournalEntryId,
                    EntryDate = jel.JournalEntry.EntryDate,
                    SourceTable = jel.JournalEntry.SourceTable ?? "journal",
                    Description = jel.JournalEntry.Description ?? jel.Narration ?? "Voucher Entry",
                    AccountName = jel.Account != null ? jel.Account.Name : "Account",
                    AccountCode = jel.Account != null ? jel.Account.AccountCode : "",
                    Amount = jel.Debit > 0 ? jel.Debit : jel.Credit,
                    TransactionType = jel.Debit > 0 ? "Debit" : "Credit"
                })
                .ToListAsync();

            return new AdminDashboardStatsDto
            {
                CashBalance = cashBalance,
                BankBalance = bankBalance,
                PendingPayments = pendingPayments,
                PendingPaymentsCount = pendingPaymentsCount,
                DueReceipts = dueReceipts,
                DueReceiptsCount = dueReceiptsCount,
                TodayCashChange = todayCashChange,
                TodayBankChange = todayBankChange,
                TodayPurchaseLiters = Math.Round(todayPurchaseLiters, 2),
                TodayPurchaseAmount = Math.Round(todayPurchaseAmount, 2),
                TodaySalesLiters = Math.Round(todaySalesLiters, 2),
                TodaySalesAmount = Math.Round(todaySalesAmount, 2),
                MonthlyTrends = monthlyTrends,
                RecentTransactions = recentLines
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
