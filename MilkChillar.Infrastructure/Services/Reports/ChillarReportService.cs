using MilkChillar.Application;
using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Interfaces;
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
        private readonly IDateTimeFilterService _dateTimeFilterService;
        private readonly IStockCalculationService _stockCalculationService;

        public ChillarReportService(
            ApplicationDbContext dbContext,
            IDateTimeFilterService dateTimeFilterService,
            IStockCalculationService stockCalculationService)
        {
            _dbContext = dbContext;
            _dateTimeFilterService = dateTimeFilterService;
            _stockCalculationService = stockCalculationService;
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
            var dateStart = query.StartDate;
            var dateEnd = query.EndDate;

            // Build base queries
            var receivesQuery = BuildChillarReceivesQuery(query, tenantId);
            var salesQuery = BuildChillarSalesQuery(query, tenantId);

            // Apply time filters using the helper service
            receivesQuery = _dateTimeFilterService.ApplyChillarReceiveTimeFilter(
                receivesQuery, dateStart, dateEnd, query.StartTimeOfDay, query.EndTimeOfDay);

            salesQuery = _dateTimeFilterService.ApplySalesTimeFilter(
                salesQuery, dateStart, dateEnd, query.StartTimeOfDay, query.EndTimeOfDay);

            // Calculate previous stock using the helper service
            var previousStockLiters = await _stockCalculationService.CalculatePreviousStockAsync(
                query.ChillarId ?? 0, dateStart, query.StartTimeOfDay, tenantId);

            // Calculate current range totals (within selected date range)
            var totalReceives = await receivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSales = await salesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;

            // Current Stock = Previous Stock + Total Receive - Total Sales
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

            // Apply date range filters
            if (query.StartDate != default)
                receives = receives.Where(r => r.Date >= query.StartDate);

            if (query.EndDate != default)
                receives = receives.Where(r => r.Date <= query.EndDate);

            // Apply other filters
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

            // Apply date range filters
            if (query.StartDate != default)
                sales = sales.Where(s => s.Date >= query.StartDate);

            if (query.EndDate != default)
                sales = sales.Where(s => s.Date <= query.EndDate);

            // Apply other filters
            if (query.ChillarId.HasValue)
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);

            return sales;
        }

        private IQueryable<ChillarReceive> ApplyDateAndTimeFilters(
            IQueryable<ChillarReceive> receives,
            ChillarInchargeDashboardQuery query)
        {
            // Now delegated to IDateTimeFilterService in GetChillarInchargeDashboardStatsAsync
            // Kept for backward compatibility if used elsewhere
            if (query.StartDate != default)
            {
                receives = receives.Where(r => r.Date >= query.StartDate);
            }

            if (query.EndDate != default)
            {
                receives = receives.Where(r => r.Date <= query.EndDate);
            }

            return receives;
        }
    }
}
