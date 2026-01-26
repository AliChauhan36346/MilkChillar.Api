using MilkChillar.Application;
using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Parameters;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for chillar receive and dodhi purchase dashboard reports
    /// </summary>
    public class ChillarReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public ChillarReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardStatsQuery query, int tenantId)
        {
            var purchases = BuildPurchasesQuery(query, tenantId);
            var receives = BuildReceivesQuery(query, tenantId);

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
            var purchaseDtos = await BuildPurchasesQuery(query, tenantId)
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

            var receiveDtos = await BuildReceivesQuery(query, tenantId)
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
            var receives = BuildChillarReceivesQuery(query, tenantId);
            var sales = BuildChillarSalesQuery(query, tenantId);

            // Calculate previous stock
            var previousStockLiters = await CalculatePreviousStockAsync(query, tenantId);

            // Calculate current range totals
            var totalReceives = await receives.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSales = await sales.SumAsync(s => (decimal?)s.NetLiters) ?? 0m;

            var currentStock = ReportCalculationHelper.CalculateNetBalance(previousStockLiters, totalReceives, totalSales);

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
            var receives = BuildChillarReceivesQuery(query, tenantId);

            // Apply date and time filters
            receives = ApplyDateAndTimeFilters(receives, query);

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

        // Private helpers
        private IQueryable<Purchase> BuildPurchasesQuery(DashboardStatsQuery query, int tenantId)
        {
            var purchases = _dbContext.Purchases.Where(p => p.TenantId == tenantId &&
                                                       p.Date >= query.StartDate &&
                                                       p.Date <= query.EndDate);

            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                purchases = purchases.Where(p => p.TimeOfDay.ToLower() == time);
            }

            if (query.DodhiId.HasValue)
            {
                purchases = purchases.Where(p => p.DodhiId == query.DodhiId.Value);
            }

            if (query.ChillarId.HasValue)
            {
                var dodhiIds = _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == query.ChillarId.Value)
                    .Select(e => e.EmployeeId)
                    .ToList();

                purchases = purchases.Where(p => dodhiIds.Contains(p.DodhiId));
            }

            return purchases;
        }

        private IQueryable<ChillarReceive> BuildReceivesQuery(DashboardStatsQuery query, int tenantId)
        {
            var receives = _dbContext.ChillarReceives.Where(r => r.TenantId == tenantId &&
                                                       r.Date >= query.StartDate &&
                                                       r.Date <= query.EndDate);

            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                receives = receives.Where(r => r.TimeOfDay.ToLower() == time);
            }

            if (query.DodhiId.HasValue)
            {
                receives = receives.Where(r => r.DodhiId == query.DodhiId.Value);
            }

            if (query.ChillarId.HasValue)
            {
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);
            }

            return receives;
        }

        private IQueryable<ChillarReceive> BuildChillarReceivesQuery(ChillarInchargeDashboardQuery query, int tenantId)
        {
            var receives = _dbContext.ChillarReceives.Where(r => r.TenantId == tenantId);

            if (query.ChillarId.HasValue)
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);

            if (query.ChillarInchargeId.HasValue)
                receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);

            if (query.DodhiId.HasValue)
                receives = receives.Where(r => r.DodhiId == query.DodhiId.Value);

            return receives;
        }

        private IQueryable<Sales> BuildChillarSalesQuery(ChillarInchargeDashboardQuery query, int tenantId)
        {
            var sales = _dbContext.Sales.Where(s => s.TenantId == tenantId);

            if (query.ChillarId.HasValue)
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);

            return sales;
        }

        private IQueryable<ChillarReceive> ApplyDateAndTimeFilters(
            IQueryable<ChillarReceive> receives,
            ChillarInchargeDashboardQuery query)
        {
            if (query.StartDate == default || query.EndDate == default)
                return receives;

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

            return receives;
        }

        private async Task<decimal> CalculatePreviousStockAsync(ChillarInchargeDashboardQuery query, int tenantId)
        {
            var receives = _dbContext.ChillarReceives.Where(r =>
                r.TenantId == tenantId &&
                r.Date < query.StartDate);

            if (query.ChillarId.HasValue)
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);

            var sales = _dbContext.Sales.Where(s =>
                s.TenantId == tenantId &&
                s.Date < query.StartDate);

            if (query.ChillarId.HasValue)
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);

            // Handle morning on start date
            if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && 
                query.StartTimeOfDay.ToLower() == "evening")
            {
                receives = receives.Concat(_dbContext.ChillarReceives.Where(r =>
                    r.TenantId == tenantId &&
                    r.Date == query.StartDate &&
                    r.TimeOfDay.ToLower() == "morning"));

                sales = sales.Concat(_dbContext.Sales.Where(s =>
                    s.TenantId == tenantId &&
                    s.Date == query.StartDate));
            }

            var totalReceives = await receives.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSales = await sales.SumAsync(s => (decimal?)s.NetLiters) ?? 0m;

            return totalReceives - totalSales;
        }
    }
}
