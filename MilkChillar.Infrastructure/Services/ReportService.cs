using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.Responses;

namespace MilkChillar.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardStatsQuery query, int tenantId)
        {
            var purchases = _dbContext.Purchases.AsQueryable();
            var receives = _dbContext.ChillarReceives.AsQueryable();
            var employees = _dbContext.Employees.AsQueryable();

            // Tenant and Date filter
            purchases = purchases.Where(p => p.TenantId == tenantId && p.Date >= query.StartDate && p.Date <= query.EndDate);
            receives = receives.Where(r => r.TenantId == tenantId && r.Date >= query.StartDate && r.Date <= query.EndDate);

            // Time of day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                purchases = purchases.Where(p => p.TimeOfDay.ToLower() == time);
                receives = receives.Where(r => r.TimeOfDay.ToLower() == time);
            }

            // Dodhi Filter
            if (query.DodhiId.HasValue)
            {
                purchases = purchases.Where(p => p.DodhiId == query.DodhiId);
                receives = receives.Where(r => r.DodhiId == query.DodhiId);
            }

            // ChillarId filter (by dodhi association)
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;

                // All employees (dodhis) assigned to this chillar
                var dodhiIds = await employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchases = purchases.Where(p => dodhiIds.Contains(p.DodhiId));
                receives = receives.Where(r => r.ChillarId == chillarId);
            }

            var totalPurchaseLiters = await purchases.SumAsync(p => p.GrossLiters);
            var totalReceiveLiters = await receives.SumAsync(r => r.GrossLiters);

            return new DashboardStatsDto
            {
                TotalPurchaseLiters = totalPurchaseLiters,
                TotalReceivedLiters = totalReceiveLiters
            };
        }

        public async Task<DodhiPurchaseReportDto> GetDashboardRecordsAsync(DashboardStatsQuery query, int tenantId)
        {
            var purchasesQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= query.StartDate &&
                            p.Date <= query.EndDate);

            var receivesQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date >= query.StartDate &&
                            r.Date <= query.EndDate);

            // Time of Day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                purchasesQuery = purchasesQuery.Where(p => p.TimeOfDay.ToLower() == time);
                receivesQuery = receivesQuery.Where(r => r.TimeOfDay.ToLower() == time);
            }

            // Dodhi filter
            if (query.DodhiId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.DodhiId == query.DodhiId.Value);
                receivesQuery = receivesQuery.Where(r => r.DodhiId == query.DodhiId.Value);
            }

            // Chillar filter (by dodhi association)
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;

                var dodhiIds = await _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
                receivesQuery = receivesQuery.Where(r => r.ChillarId == chillarId);
            }

            // Project to DTOs
            var purchaseDtos = await purchasesQuery
                .Select(p => new PurchaseDto
                {
                    PurchaseId = p.PurchaseId,
                    Date = p.Date,
                    TimeOfDay = p.TimeOfDay,
                    AccountId = p.AccountId,
                    AccountName = p.Account.Name,
                    AccountCode = p.Account.AccountCode,
                    ExpenseAccountName = p.ExpenseAccount.Name,
                    DodhiName = p.Dodhi.FullName,
                    GrossLiters = p.GrossLiters,
                    Rate = p.Rate,
                    TotalAmount = p.GrossLiters * p.Rate,
                    Balance = p.Balance
                })
                .ToListAsync();

            var receiveDtos = await receivesQuery
                .Select(r => new ChillarReceiveDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = r.Date,
                    TimeOfDay = r.TimeOfDay,
                    ChillarName = r.Chillar.Name,
                    InchargeName = r.ChillarIncharge.FullName,
                    DodhiName = r.Dodhi.FullName,
                    dodhiID = r.DodhiId,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .ToListAsync();

            return new DodhiPurchaseReportDto
            {
                Purchases = purchaseDtos,
                Receives = receiveDtos
            };
        }

        public async Task<ChillarInchargeDashboardStatsDto> GetChillarInchargeDashboardStatsAsync(
    ChillarInchargeDashboardQuery query,
    int tenantId)
        {
            var receives = _dbContext.ChillarReceives.AsQueryable();
            var sales = _dbContext.Sales.AsQueryable();

            // Filter by tenant
            receives = receives.Where(r => r.TenantId == tenantId);
            sales = sales.Where(s => s.TenantId == tenantId);

            // ChillarId filter
            if (query.ChillarId.HasValue)
            {
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
            }

            // ChillarIncharge filter
            if (query.ChillarInchargeId.HasValue)
            {
                receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);
            }

            // Previous stock calculation - FIXED
            var prevReceivesQuery = receives.Where(r => r.Date < query.StartDate);
            var prevSalesQuery = sales.Where(s => s.Date < query.StartDate);

            // Handle StartDate adjustments for previous stock
            if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "evening")
            {
                // If starting from evening, add morning receives and all sales from StartDate to previous stock
                prevReceivesQuery = prevReceivesQuery
                    .Concat(receives.Where(r => r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                prevSalesQuery = prevSalesQuery
                    .Concat(sales.Where(s => s.Date == query.StartDate));
            }

            // Previous stock calculation - Build base queries
            var prevReceivesQueryb = receives.Where(r => r.Date < query.StartDate);
            var prevSalesQueryb = sales.Where(s => s.Date < query.StartDate);

            // Handle StartDate adjustments for previous stock
            if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "evening")
            {
                // If starting from evening, add morning receives and all sales from StartDate to previous stock
                prevReceivesQuery = prevReceivesQuery
                    .Concat(receives.Where(r => r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                prevSalesQuery = prevSalesQuery
                    .Concat(sales.Where(s => s.Date == query.StartDate));
            }

            // Calculate previous stock properly: Receives - Sales
            var prevReceivesTotal = await prevReceivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var prevSalesTotal = await prevSalesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;
            var previousStockLiters = prevReceivesTotal - prevSalesTotal;

            // Debug logging to identify the issue
            var debugPrevReceivesCount = await prevReceivesQuery.CountAsync();
            var debugPrevSalesCount = await prevSalesQuery.CountAsync();
            System.Diagnostics.Debug.WriteLine($"Debug - StartDate: {query.StartDate}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Receives Count: {debugPrevReceivesCount}, Total: {prevReceivesTotal}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Sales Count: {debugPrevSalesCount}, Total: {prevSalesTotal}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Stock: {previousStockLiters}");

            // Current range receives filter
            var receivesQuery = receives.Where(r =>
                // Include dates strictly between StartDate and EndDate
                (r.Date > query.StartDate && r.Date < query.EndDate) ||

                // Handle StartDate inclusion based on StartTimeOfDay
                (r.Date == query.StartDate &&
                    (string.IsNullOrWhiteSpace(query.StartTimeOfDay) || // Include all if no time specified
                     query.StartTimeOfDay.ToLower() == "morning" || // Include all if starting from morning
                     (query.StartTimeOfDay.ToLower() == "evening" && r.TimeOfDay.ToLower() == "evening"))) || // Only evening if starting from evening

                // Handle EndDate inclusion based on EndTimeOfDay
                (r.Date == query.EndDate &&
                    (string.IsNullOrWhiteSpace(query.EndTimeOfDay) || // Include all if no end time specified
                     query.EndTimeOfDay.ToLower() == "evening" || // Include all if ending at evening
                     (query.EndTimeOfDay.ToLower() == "morning" && r.TimeOfDay.ToLower() == "morning"))) // Only morning if ending at morning
            );

            // Current range sales filter
            var salesQuery = sales.Where(s =>
                // Include dates strictly between StartDate and EndDate
                (s.Date > query.StartDate && s.Date < query.EndDate) ||

                // Handle StartDate inclusion - only if starting from morning or no time specified
                (s.Date == query.StartDate &&
                    (string.IsNullOrWhiteSpace(query.StartTimeOfDay) || query.StartTimeOfDay.ToLower() == "morning")) ||

                // Handle EndDate inclusion - CORRECTED LOGIC
                // Include ALL sales from EndDate regardless of EndTimeOfDay (since sales don't have time)
                (s.Date == query.EndDate)
            );

            var totalReceives = await receivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSales = await salesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;
            var currentStock = (previousStockLiters + totalReceives) - totalSales;

            return new ChillarInchargeDashboardStatsDto
            {
                PreviousStock = previousStockLiters,
                TotalChillarReceive = totalReceives,
                TotalSales = totalSales,
                CurrentStock = currentStock
            };
        }

        public async Task<List<ChillarReceiveDto>> GetChillarReceiveRecordsAsync(
        ChillarInchargeDashboardQuery query,
        int tenantId)
        {
            var receives = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId);

            // ✅ Filter by Chillar
            if (query.ChillarId.HasValue)
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);

            // ✅ Filter by Chillar Incharge
            if (query.ChillarInchargeId.HasValue)
                receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);

            // ✅ Filter by Dodhi
            if (query.DodhiId.HasValue)
                receives = receives.Where(r => r.DodhiId == query.DodhiId.Value);

            // ✅ Date filters with morning/evening logic
            if (query.StartDate != default && query.EndDate != default)
            {
                // Start date logic
                if (!string.IsNullOrEmpty(query.StartTimeOfDay))
                {
                    if (query.StartTimeOfDay.Equals("morning", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date > query.StartDate) ||
                            (r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (query.StartTimeOfDay.Equals("evening", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date > query.StartDate) ||
                            (r.Date == query.StartDate && r.TimeOfDay.ToLower() == "evening"));
                    }
                }
                else
                {
                    receives = receives.Where(r => r.Date >= query.StartDate);
                }

                // End date logic
                if (!string.IsNullOrEmpty(query.EndTimeOfDay))
                {
                    if (query.EndTimeOfDay.Equals("morning", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date < query.EndDate) ||
                            (r.Date == query.EndDate && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (query.EndTimeOfDay.Equals("evening", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r => r.Date <= query.EndDate);
                    }
                }
                else
                {
                    receives = receives.Where(r => r.Date <= query.EndDate);
                }
            }

            // ✅ Projection to DTO
            var result = await receives
                .Select(r => new ChillarReceiveDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = r.Date,
                    TimeOfDay = r.TimeOfDay,
                    ChillarName = r.Chillar.Name,
                    InchargeName = r.ChillarIncharge.FullName,
                    DodhiName = r.Dodhi.FullName,
                    dodhiID = r.DodhiId,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeOfDay)
                .ToListAsync();

            return result;
        }

        public async Task<List<SaleDto>> GetSalesReportAsync(SalesReportQuery query, int tenantId)
        {
            var sales = _dbContext.Sales
                .Where(s => s.TenantId == tenantId);

            if (query.ChillarId.HasValue)
            {
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
            }


            // ✅ Date Range Filter
            if (query.StartDate != default && query.EndDate != default)
            {
                sales = sales.Where(s => s.Date >= query.StartDate && s.Date <= query.EndDate);
            }

            // ✅ Buyer Code Filter
            if (!string.IsNullOrEmpty(query.BuyerCode))
            {
                sales = sales.Where(s => s.Account.AccountCode == query.BuyerCode);
            }

            // ✅ Projection
            var result = await sales
                .Select(s => new SaleDto
                {
                    SaleId = s.SaleId,
                    Date = s.Date,
                    AccountId = s.AccountId,
                    AccountCode = s.Account.AccountCode,
                    AccountName = s.Account.Name,
                    RevenueAccountName = s.RevenueAccount.Name,
                    ChillarName = s.Chillar.Name,
                    AddedByName = s.User.Username,
                    GrossLiters = s.GrossLiters,
                    LR = s.LR,
                    Fat = s.Fat,
                    NetLiters = s.NetLiters,
                    Rate = s.Rate,
                    TotalAmount = s.NetLiters * s.Rate,
                    AmountReceived = s.AmountReceived,
                    Balance = s.Balance
                })
                .OrderBy(s => s.Date)
                .ThenBy(s => s.AccountName)
                .ToListAsync();

            return result;
        }


        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int tenantId)
        {
            var today = DateTime.SpecifyKind(DateTime.Today.Date,DateTimeKind.Utc);

            // Get Cash Account Balance (account code starts with 110)
            var cashBalance = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId && ab.Account.AccountCode.StartsWith("110"))
                .Select(ab => ab.DebitTotal - ab.CreditTotal)
                .FirstOrDefaultAsync();

            // Get Bank Account Balance (account code starts with 120)
            var bankBalance = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId && ab.Account.AccountCode.StartsWith("120"))
                .Select(ab => ab.DebitTotal - ab.CreditTotal)
                .FirstOrDefaultAsync();

            // Today's Cash Change from Journal Lines (accounts starting with 110)
            var cashAccountIds = await _dbContext.Accounts
                .Where(a => a.TenantId == tenantId && a.AccountCode.StartsWith("110"))
                .Select(a => a.AccountId)
                .ToListAsync();

            var todayCashDebits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              cashAccountIds.Contains(jel.AccountId) &&
                              jel.Debit > 0)
                .SumAsync(jel => (decimal?)jel.Debit) ?? 0;

            var todayCashCredits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              cashAccountIds.Contains(jel.AccountId) &&
                              jel.Credit > 0)
                .SumAsync(jel => (decimal?)jel.Credit) ?? 0;

            var todayCashChange = todayCashDebits - todayCashCredits;

            // Today's Bank Change from Journal Lines (accounts starting with 120)
            var bankAccountIds = await _dbContext.Accounts
                .Where(a => a.TenantId == tenantId && a.AccountCode.StartsWith("120"))
                .Select(a => a.AccountId)
                .ToListAsync();

            var todayBankDebits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              bankAccountIds.Contains(jel.AccountId) &&
                              jel.Debit > 0)
                .SumAsync(jel => (decimal?)jel.Debit) ?? 0;

            var todayBankCredits = await _dbContext.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate == today &&
                              bankAccountIds.Contains(jel.AccountId) &&
                              jel.Credit > 0)
                .SumAsync(jel => (decimal?)jel.Credit) ?? 0;

            var todayBankChange = todayBankDebits - todayBankCredits;

            // Pending Payments to Suppliers (account code starts with 200)
            // Suppliers with Credit > Debit means we owe them
            var supplierBalances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith("200")
                             &&
                             ab.CreditTotal > ab.DebitTotal)
                .ToListAsync();

            var pendingPayments = supplierBalances.Sum(ab => ab.CreditTotal - ab.DebitTotal);
            var pendingPaymentsCount = supplierBalances.Count;

            // Due Receipts from Buyers (account code starts with 100)
            // Buyers with Debit > Credit means they owe us
            var buyerBalances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith("100") &&
                             ab.DebitTotal > ab.CreditTotal)
                .ToListAsync();

            var dueReceipts = buyerBalances.Sum(ab => ab.DebitTotal - ab.CreditTotal);
            var dueReceiptsCount = buyerBalances.Count;

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

        public async Task<PagedAccountBalancesDto> GetAccountBalancesAsync(
            string accountType,
            int tenantId,
            int pageNumber = 1,
            int pageSize = 25)
        {
            // Determine account code prefix based on type
            string accountCodePrefix;
            bool isSupplier = false;

            switch (accountType.ToLower())
            {
                case "supplier":
                    accountCodePrefix = "200";
                    isSupplier = true;
                    break;
                case "buyer":
                    accountCodePrefix = "100";
                    break;
                case "cash":
                    accountCodePrefix = "110";
                    break;
                case "bank":
                    accountCodePrefix = "120";
                    break;
                default:
                    throw new ArgumentException("Account type must be 'Supplier', 'Buyer', 'Cash', or 'Bank'");
            }

            var query = _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith(accountCodePrefix));

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
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
                    Balance = isSupplier
                        ? ab.CreditTotal - ab.DebitTotal  // Suppliers: Credit - Debit (we owe them)
                        : ab.DebitTotal - ab.CreditTotal,  // Buyers/Cash/Bank: Debit - Credit
                    LastUpdated = ab.LastUpdated
                })
                .ToListAsync();

            // Get summary
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
            // Determine account code prefix based on type
            string accountCodePrefix;
            bool isSupplier = false;

            switch (accountType.ToLower())
            {
                case "supplier":
                    accountCodePrefix = "200";
                    isSupplier = true;
                    break;
                case "buyer":
                    accountCodePrefix = "100";
                    break;
                case "cash":
                    accountCodePrefix = "110";
                    break;
                case "bank":
                    accountCodePrefix = "120";
                    break;
                default:
                    throw new ArgumentException("Account type must be 'Supplier', 'Buyer', 'Cash', or 'Bank'");
            }

            var balances = await _dbContext.AccountBalances
                .Include(ab => ab.Account)
                .Where(ab => ab.TenantId == tenantId &&
                             ab.Account.AccountCode.StartsWith(accountCodePrefix))
                .ToListAsync();

            var totalDebit = balances.Sum(ab => ab.DebitTotal);
            var totalCredit = balances.Sum(ab => ab.CreditTotal);
            var netBalance = isSupplier
                ? totalCredit - totalDebit  // We owe suppliers
                : totalDebit - totalCredit;  // Others owe us or our balance

            return new AccountBalanceSummaryDto
            {
                AccountType = accountType,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                NetBalance = netBalance,
                AccountCount = balances.Count
            };
        }


        // UPDATED - Remove summary calculation from this method
        public async Task<PagedPurchaseReportDto> GetDetailedPurchaseReportAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = _dbContext.Purchases
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                    .ThenInclude(d => d.Chillar)
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= DateOnly.FromDateTime(query.StartDate) &&
                            p.Date <= DateOnly.FromDateTime(query.EndDate));

            // Time of Day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                if (time == "morning" || time == "evening")
                {
                    purchasesQuery = purchasesQuery.Where(p => p.TimeOfDay.ToLower() == time);
                }
            }

            // Dodhi filter
            if (query.DodhiId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.DodhiId == query.DodhiId.Value);
            }

            // Chillar filter
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;
                var dodhiIds = await _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
            }

            // Supplier code filter
            if (!string.IsNullOrWhiteSpace(query.SupplierCode))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Account.AccountCode == query.SupplierCode);
            }

            // Get total count
            var totalCount = await purchasesQuery.CountAsync();

            // Get paginated purchases
            var purchases = await purchasesQuery
                .OrderBy(p => p.Date)
                .ThenBy(p => p.TimeOfDay)
                .ThenBy(p => p.Account.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PurchaseDetailDto
                {
                    PurchaseId = p.PurchaseId,
                    Date = p.Date.ToDateTime(TimeOnly.MinValue),
                    TimeOfDay = p.TimeOfDay,
                    AccountCode = p.Account.AccountCode,
                    AccountName = p.Account.Name,
                    ExpenseAccountName = p.ExpenseAccount.Name,
                    DodhiName = p.Dodhi.FullName,
                    DodhiId = p.DodhiId,
                    ChillarName = p.Dodhi.Chillar != null ? p.Dodhi.Chillar.Name : "",
                    GrossLiters = p.GrossLiters,
                    Rate = p.Rate,
                    TotalAmount = p.GrossLiters * p.Rate,
                    Balance = p.Balance
                })
                .ToListAsync();

            return new PagedPurchaseReportDto
            {
                PaginatedPurchases = new PaginatedResult<PurchaseDetailDto>
                {
                    Items = purchases,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                }
            };
        }

        // NEW - Separate summary endpoint
        public async Task<PurchaseReportSummaryDto> GetPurchaseReportSummaryAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= DateOnly.FromDateTime(query.StartDate) &&
                            p.Date <= DateOnly.FromDateTime(query.EndDate));

            // Apply same filters as detailed report
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                if (time == "morning" || time == "evening")
                {
                    purchasesQuery = purchasesQuery.Where(p => p.TimeOfDay.ToLower() == time);
                }
            }

            if (query.DodhiId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.DodhiId == query.DodhiId.Value);
            }

            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;
                var dodhiIds = await _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
            }

            if (!string.IsNullOrWhiteSpace(query.SupplierCode))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Account.AccountCode == query.SupplierCode);
            }

            // Calculate summary in single query
            var summaryData = await purchasesQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalLiters = g.Sum(p => p.GrossLiters),
                    TotalAmount = g.Sum(p => p.GrossLiters * p.Rate),
                    TotalTransactions = g.Count(),
                    TotalSuppliers = g.Select(p => p.AccountId).Distinct().Count()
                })
                .FirstOrDefaultAsync();

            var summary = new PurchaseReportSummaryDto
            {
                TotalLiters = summaryData?.TotalLiters ?? 0,
                TotalAmount = summaryData?.TotalAmount ?? 0,
                TotalTransactions = summaryData?.TotalTransactions ?? 0,
                TotalSuppliers = summaryData?.TotalSuppliers ?? 0
            };

            summary.AverageRate = summary.TotalLiters > 0
                ? summary.TotalAmount / summary.TotalLiters
                : 0;

            return summary;
        }

        // NEW - Buyer-wise sales report
        public async Task<BuyerWiseSalesReportDto> GetBuyerWiseSalesReportAsync(
            SalesReportQuery query,
            int tenantId)
        {
            var salesQuery = _dbContext.Sales
                .Include(s => s.Account)
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= query.StartDate &&
                            s.Date <= query.EndDate);

            // Account (Buyer) filter
            if (query.AccountId.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.AccountId == query.AccountId.Value);
            }

            // Chillar filter
            if (query.ChillarId.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.ChillarId == query.ChillarId.Value);
            }

            // Group by buyer and calculate summaries
            var buyerSummaries = await salesQuery
                .GroupBy(s => new { s.AccountId, s.Account.AccountCode, s.Account.Name })
                .Select(g => new BuyerSalesSummaryDto
                {
                    AccountId = g.Key.AccountId,
                    AccountCode = g.Key.AccountCode,
                    AccountName = g.Key.Name,
                    TotalGrossLiters = g.Sum(s => s.GrossLiters),
                    TotalNetLiters = g.Sum(s => s.NetLiters),
                    TotalAmount = g.Sum(s => s.NetLiters * s.Rate),
                    TotalAmountReceived = g.Sum(s => s.AmountReceived),
                    AverageLR = g.Average(s => s.LR ?? 0),
                    AverageFat = g.Average(s => s.Fat ?? 0),
                    TransactionCount = g.Count(),
                    Balance = g.OrderByDescending(s => s.Date)
                               .Select(s => s.Balance)
                               .FirstOrDefault()
                })
                .OrderByDescending(b => b.TotalAmount)
                .ToListAsync();

            // Calculate average rate for each buyer
            foreach (var buyer in buyerSummaries)
            {
                buyer.AverageRate = buyer.TotalNetLiters > 0
                    ? buyer.TotalAmount / buyer.TotalNetLiters
                    : 0;
            }

            // Calculate overall summary
            var overallSummary = new SalesReportSummaryDto
            {
                TotalGrossLiters = buyerSummaries.Sum(b => b.TotalGrossLiters),
                TotalNetLiters = buyerSummaries.Sum(b => b.TotalNetLiters),
                TotalAmount = buyerSummaries.Sum(b => b.TotalAmount),
                TotalAmountReceived = buyerSummaries.Sum(b => b.TotalAmountReceived),
                TotalBalance = buyerSummaries.Sum(b => b.Balance),
                TotalTransactions = buyerSummaries.Sum(b => b.TransactionCount),
                TotalBuyers = buyerSummaries.Count
            };

            overallSummary.AverageRate = overallSummary.TotalNetLiters > 0
                ? overallSummary.TotalAmount / overallSummary.TotalNetLiters
                : 0;

            overallSummary.AverageLR = buyerSummaries.Count > 0
                ? buyerSummaries.Average(b => b.AverageLR)
                : 0;

            overallSummary.AverageFat = buyerSummaries.Count > 0
                ? buyerSummaries.Average(b => b.AverageFat)
                : 0;

            return new BuyerWiseSalesReportDto
            {
                BuyerSummaries = buyerSummaries,
                OverallSummary = overallSummary
            };
        }

        // NEW - Sales report summary
        public async Task<SalesReportSummaryDto> GetSalesReportSummaryAsync(
            SalesReportQuery query,
            int tenantId)
        {
            var salesQuery = _dbContext.Sales
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= query.StartDate &&
                            s.Date <= query.EndDate);

            if (query.AccountId.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.AccountId == query.AccountId.Value);
            }

            if (query.ChillarId.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.ChillarId == query.ChillarId.Value);
            }

            var summaryData = await salesQuery
                .GroupBy(s => 1)
                .Select(g => new
                {
                    TotalGrossLiters = g.Sum(s => s.GrossLiters),
                    TotalNetLiters = g.Sum(s => s.NetLiters),
                    TotalAmount = g.Sum(s => s.NetLiters * s.Rate),
                    TotalAmountReceived = g.Sum(s => s.AmountReceived),
                    AverageLR = g.Average(s => s.LR ?? 0),
                    AverageFat = g.Average(s => s.Fat ?? 0),
                    TotalTransactions = g.Count(),
                    TotalBuyers = g.Select(s => s.AccountId).Distinct().Count(),
                    TotalBalance = g.Sum(s => s.Balance)
                })
                .FirstOrDefaultAsync();

            var summary = new SalesReportSummaryDto
            {
                TotalGrossLiters = summaryData?.TotalGrossLiters ?? 0,
                TotalNetLiters = summaryData?.TotalNetLiters ?? 0,
                TotalAmount = summaryData?.TotalAmount ?? 0,
                TotalAmountReceived = summaryData?.TotalAmountReceived ?? 0,
                TotalBalance = summaryData?.TotalBalance ?? 0,
                AverageLR = summaryData?.AverageLR ?? 0,
                AverageFat = summaryData?.AverageFat ?? 0,
                TotalTransactions = summaryData?.TotalTransactions ?? 0,
                TotalBuyers = summaryData?.TotalBuyers ?? 0
            };

            summary.AverageRate = summary.TotalNetLiters > 0
                ? summary.TotalAmount / summary.TotalNetLiters
                : 0;

            return summary;
        }

        public async Task<SupplierWisePurchaseReportDto> GetSupplierWisePurchaseReportAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = _dbContext.Purchases
                .Include(p => p.Account)
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= DateOnly.FromDateTime(query.StartDate) &&
                            p.Date <= DateOnly.FromDateTime(query.EndDate));

            // Time of Day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                if (time == "morning" || time == "evening")
                {
                    purchasesQuery = purchasesQuery.Where(p => p.TimeOfDay.ToLower() == time);
                }
            }

            // Dodhi filter
            if (query.DodhiId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.DodhiId == query.DodhiId.Value);
            }

            // Chillar filter (through dodhi association)
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;
                var dodhiIds = await _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
            }

            // Supplier code filter
            if (!string.IsNullOrWhiteSpace(query.SupplierCode))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Account.AccountCode == query.SupplierCode);
            }

            // Group by supplier and calculate summaries
            var supplierSummaries = await purchasesQuery
                .GroupBy(p => new { p.AccountId, p.Account.AccountCode, p.Account.Name })
                .Select(g => new SupplierPurchaseSummaryDto
                {
                    AccountId = g.Key.AccountId,
                    AccountCode = g.Key.AccountCode,
                    AccountName = g.Key.Name,
                    TotalLiters = g.Sum(p => p.GrossLiters),
                    TotalAmount = g.Sum(p => p.GrossLiters * p.Rate),
                    TransactionCount = g.Count(),
                    Balance = g.OrderByDescending(p => p.Date)
                               .ThenByDescending(p => p.TimeOfDay)
                               .Select(p => p.Balance)
                               .FirstOrDefault()
                })
                .OrderByDescending(s => s.TotalAmount)
                .ToListAsync();

            // Calculate average rate for each supplier
            foreach (var supplier in supplierSummaries)
            {
                supplier.AverageRate = supplier.TotalLiters > 0
                    ? supplier.TotalAmount / supplier.TotalLiters
                    : 0;
            }

            // Calculate overall summary
            var overallSummary = new PurchaseReportSummaryDto
            {
                TotalLiters = supplierSummaries.Sum(s => s.TotalLiters),
                TotalAmount = supplierSummaries.Sum(s => s.TotalAmount),
                TotalTransactions = supplierSummaries.Sum(s => s.TransactionCount),
                TotalSuppliers = supplierSummaries.Count
            };

            overallSummary.AverageRate = overallSummary.TotalLiters > 0
                ? overallSummary.TotalAmount / overallSummary.TotalLiters
                : 0;

            return new SupplierWisePurchaseReportDto
            {
                SupplierSummaries = supplierSummaries,
                OverallSummary = overallSummary
            };
        }

        public async Task<List<DailyTotalsDto>> GetDailyTotalsAsync(ProfitLossFilterRequest request, int tenantId)
        {
            var start = DateOnly.FromDateTime(request.StartDate.Date);
            var end = DateOnly.FromDateTime(request.EndDate.Date);

            // Condition for chillar filtering
            bool filterByChillar = request.ChillarId > 0;

            // Purchases
            var purchaseQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= start &&
                            p.Date <= end);

            if (filterByChillar)
                purchaseQuery = purchaseQuery.Where(p => p.Dodhi.ChillarId == request.ChillarId);

            var purchaseGroups = await purchaseQuery
                .GroupBy(p => p.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalPurchaseLiters = g.Sum(p => p.GrossLiters),
                    TotalPurchaseAmount = g.Sum(p => p.GrossLiters * p.Rate)
                })
                .ToListAsync();


            // Chillar Receive
            var receiveQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date >= start &&
                            r.Date <= end);

            if (filterByChillar)
                receiveQuery = receiveQuery.Where(r => r.ChillarId == request.ChillarId);

            var receiveGroups = await receiveQuery
                .GroupBy(r => r.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalReceiveLiters = g.Sum(r => r.GrossLiters),
                    TotalNetLiters = g.Sum(r => r.NetLiters),
                    ChillarLoss = g.Sum(r => (r.GrossLiters - r.NetLiters))
                })
                .ToListAsync();


            // Sales
            var salesQuery = _dbContext.Sales
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= start &&
                            s.Date <= end);

            if (filterByChillar)
                salesQuery = salesQuery.Where(s => s.ChillarId == request.ChillarId);

            var salesGroups = await salesQuery
                .GroupBy(s => s.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSalesLiters = g.Sum(s => s.NetLiters),
                    SalesAmount = g.Sum(s => s.NetLiters * s.Rate)
                })
                .ToListAsync();


            // Combine Final Result
            var result = new List<DailyTotalsDto>();

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var p = purchaseGroups.FirstOrDefault(x => x.Date == d);
                var r = receiveGroups.FirstOrDefault(x => x.Date == d);
                var s = salesGroups.FirstOrDefault(x => x.Date == d);

                var purchaseLiters = p?.TotalPurchaseLiters ?? 0m;
                var purchaseAmount = p?.TotalPurchaseAmount ?? 0m;

                var receiveLiters = r?.TotalReceiveLiters ?? 0m;
                var chillarLoss = r?.ChillarLoss ?? 0m;

                var salesLiters = s?.TotalSalesLiters ?? 0m;
                var salesAmount = s?.SalesAmount ?? 0m;

                var tsSalesLiters = salesLiters;
                var tsDifference = salesLiters - receiveLiters;

                var grossProfit = salesAmount - purchaseAmount;

                result.Add(new DailyTotalsDto
                {
                    Date = d,
                    TotalPurchaseLiters = purchaseLiters,
                    TotalPurchaseAmount = purchaseAmount,
                    TotalChillarReceiveLiters = receiveLiters,
                    DodhiLoss = receiveLiters - purchaseLiters,
                    TotalSalesLiters = salesLiters,
                    ChillarLoss = chillarLoss,
                    TsSalesLiters = tsSalesLiters,
                    TsDifference = tsDifference,
                    SalesAmount = salesAmount,
                    GrossProfit = grossProfit
                });
            }

            return result.OrderBy(x => x.Date).ToList();
        }

    }
}
