
using Microsoft.AspNetCore.Http;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services
{
    public class ProfitLossService : IProfitLossService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfitLossService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetTenantIdFromToken()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id");
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                throw new UnauthorizedAccessException("Tenant ID not found in token");
            }
            return tenantId;
        }

        public async Task<ProfitLossResponse> GetProfitLossReportAsync(ProfitLossFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            // Convert dates to UTC for PostgreSQL compatibility
            var startDate = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            // Get all journal entries for the period
            var journalLines = await _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                             jel.JournalEntry.EntryDate >= startDate &&
                             jel.JournalEntry.EntryDate <= endDate)
                .ToListAsync();

            // Calculate Income (Revenue accounts: 400-499)
            var incomeData = CalculateIncome(journalLines, request);

            // Calculate COGS (Cost of Sales accounts: 500-599)
            var cogsData = CalculateCogs(journalLines, request);

            // Calculate Gross Profit
            decimal grossProfit = incomeData.TotalIncome - cogsData.TotalCogs;
            decimal grossProfitMargin = incomeData.TotalIncome > 0
                ? (grossProfit / incomeData.TotalIncome) * 100
                : 0;

            // Calculate Operating Expenses (600-699)
            var operatingExpenses = CalculateOperatingExpenses(journalLines);

            // Calculate Financial Expenses (assuming 700-799)
            var financialExpenses = CalculateFinancialExpenses(journalLines);

            // Calculate Total Expenses and Net Profit
            decimal totalExpenses = operatingExpenses.TotalOperatingExpenses + financialExpenses.TotalFinancialExpenses;
            decimal netProfit = grossProfit - totalExpenses;
            decimal netProfitMargin = incomeData.TotalIncome > 0
                ? (netProfit / incomeData.TotalIncome) * 100
                : 0;

            return new ProfitLossResponse
            {
                Period = new PeriodDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DisplayText = $"{request.StartDate:dd MMM yyyy} - {request.EndDate:dd MMM yyyy}"
                },
                Income = incomeData,
                Cogs = cogsData,
                GrossProfit = grossProfit,
                GrossProfitMargin = Math.Round(grossProfitMargin, 2),
                OperatingExpenses = operatingExpenses,
                FinancialExpenses = financialExpenses,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                NetProfitMargin = Math.Round(netProfitMargin, 2)
            };
        }

        public async Task<ProfitLossComparativeResponse> GetComparativeProfitLossAsync(ComparativeProfitLossRequest request)
        {
            var period1 = await GetProfitLossReportAsync(new ProfitLossFilterRequest
            {
                StartDate = request.Period1StartDate,
                EndDate = request.Period1EndDate
            });

            var period2 = await GetProfitLossReportAsync(new ProfitLossFilterRequest
            {
                StartDate = request.Period2StartDate,
                EndDate = request.Period2EndDate
            });

            var revenueChange = period2.Income.TotalIncome - period1.Income.TotalIncome;
            var revenueChangePercentage = period1.Income.TotalIncome > 0
                ? (revenueChange / period1.Income.TotalIncome) * 100
                : 0;

            var expenseChange = period2.TotalExpenses - period1.TotalExpenses;
            var expenseChangePercentage = period1.TotalExpenses > 0
                ? (expenseChange / period1.TotalExpenses) * 100
                : 0;

            var netProfitChange = period2.NetProfit - period1.NetProfit;
            var netProfitChangePercentage = period1.NetProfit != 0
                ? (netProfitChange / Math.Abs(period1.NetProfit)) * 100
                : 0;

            return new ProfitLossComparativeResponse
            {
                Period1 = period1,
                Period2 = period2,
                Comparison = new ComparisonDto
                {
                    RevenueChange = revenueChange,
                    RevenueChangePercentage = Math.Round(revenueChangePercentage, 2),
                    ExpenseChange = expenseChange,
                    ExpenseChangePercentage = Math.Round(expenseChangePercentage, 2),
                    NetProfitChange = netProfitChange,
                    NetProfitChangePercentage = Math.Round(netProfitChangePercentage, 2)
                }
            };
        }

        public async Task<ExpenseBreakdownResponse> GetExpenseBreakdownAsync(ProfitLossFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();
            // Convert dates to UTC for PostgreSQL compatibility
            var startDate = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var journalLines = await _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                             jel.JournalEntry.EntryDate >= startDate &&
                             jel.JournalEntry.EntryDate <= endDate)
                .ToListAsync();

            // Get expense lines (600-799)
            var expenseLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("6") ||
                             jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("7"))
                .ToList();

            var categories = expenseLines
                .GroupBy(jel => jel.Account.SubAccount.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(jel => jel.Debit - jel.Credit),
                    Accounts = g.GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                        .Select(ag => new AccountBreakdownDto
                        {
                            AccountCode = ag.Key.FullCode,
                            AccountName = ag.Key.Name,
                            Debit = ag.Sum(jel => jel.Debit),
                            Credit = ag.Sum(jel => jel.Credit),
                            Balance = ag.Sum(jel => jel.Debit - jel.Credit)
                        })
                        .ToList()
                })
                .ToList();

            var totalExpenses = categories.Sum(c => c.Amount);

            return new ExpenseBreakdownResponse
            {
                Period = new PeriodDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DisplayText = $"{request.StartDate:dd MMM yyyy} - {request.EndDate:dd MMM yyyy}"
                },
                Categories = categories.Select(c => new ExpenseCategoryDto
                {
                    CategoryName = c.CategoryName,
                    Amount = c.Amount,
                    Percentage = totalExpenses > 0 ? Math.Round((c.Amount / totalExpenses) * 100, 2) : 0,
                    Accounts = c.Accounts
                }).ToList(),
                TotalExpenses = totalExpenses
            };
        }

        public async Task<IncomeBreakdownResponse> GetIncomeBreakdownAsync(ProfitLossFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            // Convert dates to UTC for PostgreSQL compatibility
            var startDate = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var journalLines = await _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                             jel.JournalEntry.EntryDate >= startDate &&
                             jel.JournalEntry.EntryDate <= endDate)
                .ToListAsync();

            // Get income lines (400-499)
            var incomeLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("4"))
                .ToList();

            var categories = incomeLines
                .GroupBy(jel => jel.Account.SubAccount.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(jel => jel.Credit - jel.Debit),
                    Accounts = g.GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                        .Select(ag => new AccountBreakdownDto
                        {
                            AccountCode = ag.Key.FullCode,
                            AccountName = ag.Key.Name,
                            Debit = ag.Sum(jel => jel.Debit),
                            Credit = ag.Sum(jel => jel.Credit),
                            Balance = ag.Sum(jel => jel.Credit - jel.Debit)
                        })
                        .ToList()
                })
                .ToList();

            var totalIncome = categories.Sum(c => c.Amount);

            return new IncomeBreakdownResponse
            {
                Period = new PeriodDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DisplayText = $"{request.StartDate:dd MMM yyyy} - {request.EndDate:dd MMM yyyy}"
                },
                Categories = categories.Select(c => new IncomeCategoryDto
                {
                    CategoryName = c.CategoryName,
                    Amount = c.Amount,
                    Percentage = totalIncome > 0 ? Math.Round((c.Amount / totalIncome) * 100, 2) : 0,
                    Accounts = c.Accounts
                }).ToList(),
                TotalIncome = totalIncome
            };
        }

        public async Task<byte[]> ExportProfitLossAsync(ProfitLossFilterRequest request, string format)
        {
            // TODO: Implement export functionality
            throw new NotImplementedException("Export functionality will be implemented based on your preferred library");
        }

        // Helper Methods (All synchronous - no async/await needed)
        private IncomeDto CalculateIncome(List<JournalEntryLine> journalLines, ProfitLossFilterRequest request)
        {
            // Revenue accounts: 400-499
            var incomeLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("4"))
                .ToList();

            var salesRevenue = incomeLines
                .Where(jel => !jel.Account.Name.ToLower().Contains("return") && !jel.Account.Name.ToLower().Contains("discount"))
                .Sum(jel => jel.Credit - jel.Debit);

            var salesReturns = incomeLines
                .Where(jel => jel.Account.Name.ToLower().Contains("return") || jel.Account.Name.ToLower().Contains("discount"))
                .Sum(jel => jel.Debit - jel.Credit);

            var otherIncome = incomeLines
                .Where(jel => jel.Account.Name.ToLower().Contains("other") || jel.Account.Name.ToLower().Contains("misc"))
                .Sum(jel => jel.Credit - jel.Debit);

            var netSales = salesRevenue - salesReturns;
            var totalIncome = netSales + otherIncome;

            var details = incomeLines
                .GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                .Select(g => new AccountBreakdownDto
                {
                    AccountCode = g.Key.FullCode,
                    AccountName = g.Key.Name,
                    Debit = g.Sum(jel => jel.Debit),
                    Credit = g.Sum(jel => jel.Credit),
                    Balance = g.Sum(jel => jel.Credit - jel.Debit)
                })
                .ToList();

            return new IncomeDto
            {
                SalesRevenue = salesRevenue,
                SalesReturns = salesReturns,
                NetSales = netSales,
                OtherIncome = otherIncome,
                TotalIncome = totalIncome,
                Details = details
            };
        }

        private CogsDto CalculateCogs(List<JournalEntryLine> journalLines, ProfitLossFilterRequest request)
        {
            // COGS accounts: 500-599
            var cogsLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("5"))
                .ToList();

            // TODO: Implement opening and closing stock logic
            // For now, assuming they are tracked in specific accounts
            var openingStock = cogsLines
                .Where(jel => jel.Account.Name.ToLower().Contains("opening stock"))
                .Sum(jel => jel.Debit - jel.Credit);

            var purchases = cogsLines
                .Where(jel => jel.Account.Name.ToLower().Contains("purchase") &&
                             !jel.Account.Name.ToLower().Contains("return"))
                .Sum(jel => jel.Debit - jel.Credit);

            var purchaseReturns = cogsLines
                .Where(jel => jel.Account.Name.ToLower().Contains("purchase") &&
                             jel.Account.Name.ToLower().Contains("return"))
                .Sum(jel => jel.Credit - jel.Debit);

            var directExpenses = cogsLines
                .Where(jel => jel.Account.Name.ToLower().Contains("direct") ||
                             jel.Account.Name.ToLower().Contains("freight") ||
                             jel.Account.Name.ToLower().Contains("carriage"))
                .Sum(jel => jel.Debit - jel.Credit);

            var closingStock = cogsLines
                .Where(jel => jel.Account.Name.ToLower().Contains("closing stock"))
                .Sum(jel => jel.Debit - jel.Credit);

            var totalCogs = openingStock + purchases - purchaseReturns + directExpenses - closingStock;

            var details = cogsLines
                .GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                .Select(g => new AccountBreakdownDto
                {
                    AccountCode = g.Key.FullCode,
                    AccountName = g.Key.Name,
                    Debit = g.Sum(jel => jel.Debit),
                    Credit = g.Sum(jel => jel.Credit),
                    Balance = g.Sum(jel => jel.Debit - jel.Credit)
                })
                .ToList();

            return new CogsDto
            {
                OpeningStock = openingStock,
                Purchases = purchases,
                PurchaseReturns = purchaseReturns,
                DirectExpenses = directExpenses,
                ClosingStock = closingStock,
                TotalCogs = totalCogs,
                Details = details
            };
        }

        private OperatingExpensesDto CalculateOperatingExpenses(List<JournalEntryLine> journalLines)
        {
            // Operating Expenses accounts: 600-699
            var expenseLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("6"))
                .ToList();

            var salaries = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("salary") ||
                             jel.Account.Name.ToLower().Contains("wage") ||
                             jel.Account.Name.ToLower().Contains("payroll"))
                .Sum(jel => jel.Debit - jel.Credit);

            var rent = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("rent"))
                .Sum(jel => jel.Debit - jel.Credit);

            var utilities = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("utility") ||
                             jel.Account.Name.ToLower().Contains("electricity") ||
                             jel.Account.Name.ToLower().Contains("water") ||
                             jel.Account.Name.ToLower().Contains("gas"))
                .Sum(jel => jel.Debit - jel.Credit);

            var transportation = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("transport") ||
                             jel.Account.Name.ToLower().Contains("vehicle") ||
                             jel.Account.Name.ToLower().Contains("fuel"))
                .Sum(jel => jel.Debit - jel.Credit);

            var marketing = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("marketing") ||
                             jel.Account.Name.ToLower().Contains("advertising") ||
                             jel.Account.Name.ToLower().Contains("promotion"))
                .Sum(jel => jel.Debit - jel.Credit);

            var officeExpenses = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("office") ||
                             jel.Account.Name.ToLower().Contains("stationary") ||
                             jel.Account.Name.ToLower().Contains("supplies"))
                .Sum(jel => jel.Debit - jel.Credit);

            var depreciation = expenseLines
                .Where(jel => jel.Account.Name.ToLower().Contains("depreciation"))
                .Sum(jel => jel.Debit - jel.Credit);

            var otherExpenses = expenseLines
                .Where(jel => !jel.Account.Name.ToLower().Contains("salary") &&
                             !jel.Account.Name.ToLower().Contains("wage") &&
                             !jel.Account.Name.ToLower().Contains("rent") &&
                             !jel.Account.Name.ToLower().Contains("utility") &&
                             !jel.Account.Name.ToLower().Contains("transport") &&
                             !jel.Account.Name.ToLower().Contains("marketing") &&
                             !jel.Account.Name.ToLower().Contains("office") &&
                             !jel.Account.Name.ToLower().Contains("depreciation"))
                .Sum(jel => jel.Debit - jel.Credit);

            var total = salaries + rent + utilities + transportation + marketing + officeExpenses + depreciation + otherExpenses;

            var details = expenseLines
                .GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                .Select(g => new AccountBreakdownDto
                {
                    AccountCode = g.Key.FullCode,
                    AccountName = g.Key.Name,
                    Debit = g.Sum(jel => jel.Debit),
                    Credit = g.Sum(jel => jel.Credit),
                    Balance = g.Sum(jel => jel.Debit - jel.Credit)
                })
                .ToList();

            return new OperatingExpensesDto
            {
                Salaries = salaries,
                Rent = rent,
                Utilities = utilities,
                Transportation = transportation,
                Marketing = marketing,
                OfficeExpenses = officeExpenses,
                Depreciation = depreciation,
                OtherExpenses = otherExpenses,
                TotalOperatingExpenses = total,
                Details = details
            };
        }

        private FinancialExpensesDto CalculateFinancialExpenses(List<JournalEntryLine> journalLines)
        {
            // Financial Expenses: Assuming 700-799 or specific accounts within 600-699
            var financialLines = journalLines
                .Where(jel => jel.Account.SubAccount.MainAccount.MainAccountCode.StartsWith("7") ||
                             jel.Account.Name.ToLower().Contains("interest") ||
                             jel.Account.Name.ToLower().Contains("bank charges") ||
                             jel.Account.Name.ToLower().Contains("financial"))
                .ToList();

            var interestExpense = financialLines
                .Where(jel => jel.Account.Name.ToLower().Contains("interest"))
                .Sum(jel => jel.Debit - jel.Credit);

            var bankCharges = financialLines
                .Where(jel => jel.Account.Name.ToLower().Contains("bank") && jel.Account.Name.ToLower().Contains("charge"))
                .Sum(jel => jel.Debit - jel.Credit);

            var otherFinancial = financialLines
                .Where(jel => !jel.Account.Name.ToLower().Contains("interest") &&
                             !(jel.Account.Name.ToLower().Contains("bank") && jel.Account.Name.ToLower().Contains("charge")))
                .Sum(jel => jel.Debit - jel.Credit);

            var total = interestExpense + bankCharges + otherFinancial;

            var details = financialLines
                .GroupBy(jel => new { jel.Account.AccountId, jel.Account.FullCode, jel.Account.Name })
                .Select(g => new AccountBreakdownDto
                {
                    AccountCode = g.Key.FullCode,
                    AccountName = g.Key.Name,
                    Debit = g.Sum(jel => jel.Debit),
                    Credit = g.Sum(jel => jel.Credit),
                    Balance = g.Sum(jel => jel.Debit - jel.Credit)
                })
                .ToList();

            return new FinancialExpensesDto
            {
                InterestExpense = interestExpense,
                BankCharges = bankCharges,
                OtherFinancialExpenses = otherFinancial,
                TotalFinancialExpenses = total,
                Details = details
            };
        }
    }
}