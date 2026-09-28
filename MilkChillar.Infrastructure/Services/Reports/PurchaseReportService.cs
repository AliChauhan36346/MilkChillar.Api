using MilkChillar.Application;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for purchase-related reports
    /// </summary>
    public class PurchaseReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public PurchaseReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedPurchaseReportDto> GetDetailedPurchaseReportAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = BuildPurchaseQuery(query, tenantId);
            var totalCount = await purchasesQuery.CountAsync();

            var purchaseEntities = await purchasesQuery
                .OrderBy(p => p.Date)
                .ThenBy(p => p.TimeOfDay)
                .ThenBy(p => p.Account.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var purchases = purchaseEntities.Select(MapToPurchaseDetailDto).ToList();

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

        public async Task<PurchaseReportSummaryDto> GetPurchaseReportSummaryAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = BuildPurchaseQuery(query, tenantId);

            var totalTransactions = await purchasesQuery.CountAsync();
            if (totalTransactions == 0)
            {
                return new PurchaseReportSummaryDto
                {
                    TotalLiters = 0,
                    TotalAmount = 0,
                    TotalTransactions = 0,
                    TotalSuppliers = 0,
                    AverageRate = 0
                };
            }

            var totalLiters = await purchasesQuery.SumAsync(p => p.GrossLiters);
            var totalAmount = await purchasesQuery.SumAsync(p => p.GrossLiters * p.Rate);
            var totalSuppliers = await purchasesQuery.Select(p => p.AccountId).Distinct().CountAsync();

            var summary = new PurchaseReportSummaryDto
            {
                TotalLiters = totalLiters,
                TotalAmount = totalAmount,
                TotalTransactions = totalTransactions,
                TotalSuppliers = totalSuppliers
            };

            summary.AverageRate = ReportCalculationHelper.CalculateAverageRate(summary.TotalAmount, summary.TotalLiters);

            return summary;
        }

        public async Task<SupplierWisePurchaseReportDto> GetSupplierWisePurchaseReportAsync(
            PurchaseReportQuery query,
            int tenantId)
        {
            var purchasesQuery = BuildPurchaseQuery(query, tenantId);

            var supplierAggregates = await purchasesQuery
                .GroupBy(p => new { p.AccountId, p.Account.AccountCode, p.Account.Name })
                .Select(g => new
                {
                    AccountId = g.Key.AccountId,
                    AccountCode = g.Key.AccountCode,
                    AccountName = g.Key.Name,
                    TotalLiters = g.Sum(p => p.GrossLiters),
                    TotalAmount = g.Sum(p => p.GrossLiters * p.Rate),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(s => s.TotalAmount)
                .ToListAsync();

            if (supplierAggregates.Count == 0)
            {
                return new SupplierWisePurchaseReportDto
                {
                    SupplierSummaries = new List<SupplierPurchaseSummaryDto>(),
                    OverallSummary = new PurchaseReportSummaryDto()
                };
            }

            // Get the latest balance for each supplier account in this query set
            var latestBalances = await purchasesQuery
                .OrderByDescending(p => p.Date)
                .ThenByDescending(p => p.TimeOfDay)
                .Select(p => new { p.AccountId, p.Balance })
                .ToListAsync();

            var balanceByAccount = latestBalances
                .GroupBy(x => x.AccountId)
                .ToDictionary(g => g.Key, g => g.First().Balance);

            var supplierSummaries = supplierAggregates.Select(s => new SupplierPurchaseSummaryDto
            {
                AccountId = s.AccountId,
                AccountCode = s.AccountCode,
                AccountName = s.AccountName,
                TotalLiters = s.TotalLiters,
                TotalAmount = s.TotalAmount,
                TransactionCount = s.TransactionCount,
                Balance = balanceByAccount.TryGetValue(s.AccountId, out var bal) ? bal : 0,
                AverageRate = ReportCalculationHelper.CalculateAverageRate(s.TotalAmount, s.TotalLiters)
            }).ToList();

            // Calculate overall summary
            var overallSummary = new PurchaseReportSummaryDto
            {
                TotalLiters = supplierSummaries.Sum(s => s.TotalLiters),
                TotalAmount = supplierSummaries.Sum(s => s.TotalAmount),
                TotalTransactions = supplierSummaries.Sum(s => s.TransactionCount),
                TotalSuppliers = supplierSummaries.Count
            };

            overallSummary.AverageRate = ReportCalculationHelper.CalculateAverageRate(overallSummary.TotalAmount, overallSummary.TotalLiters);

            return new SupplierWisePurchaseReportDto
            {
                SupplierSummaries = supplierSummaries,
                OverallSummary = overallSummary
            };
        }

        // Private helpers
        private IQueryable<Purchase> BuildPurchaseQuery(PurchaseReportQuery query, int tenantId)
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
                var dodhiIds = _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == query.ChillarId.Value)
                    .Select(e => e.EmployeeId)
                    .ToList();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
            }

            // Supplier code filter
            if (!string.IsNullOrWhiteSpace(query.SupplierCode))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Account.AccountCode == query.SupplierCode);
            }

            return purchasesQuery;
        }

        private PurchaseDetailDto MapToPurchaseDetailDto(Purchase p)
        {
            return new PurchaseDetailDto
            {
                PurchaseId = p.PurchaseId,
                Date = p.Date.ToDateTime(TimeOnly.MinValue),
                TimeOfDay = p.TimeOfDay,
                AccountCode = p.Account != null ? p.Account.AccountCode : "",
                AccountName = p.Account != null ? p.Account.Name : "",
                ExpenseAccountName = p.ExpenseAccount != null ? p.ExpenseAccount.Name : "",
                DodhiName = p.Dodhi != null ? p.Dodhi.FullName : "",
                DodhiId = p.DodhiId,
                ChillarName = p.Dodhi?.Chillar != null ? p.Dodhi.Chillar.Name : "",
                GrossLiters = p.GrossLiters,
                Rate = p.Rate,
                TotalAmount = p.GrossLiters * p.Rate,
                Balance = p.Balance
            };
        }
    }
}
