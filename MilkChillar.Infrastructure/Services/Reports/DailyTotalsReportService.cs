using MilkChillar.Application;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for daily totals report with optional time filters and stock calculation
    /// </summary>
    public class DailyTotalsReportService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeFilterService _dateTimeFilterService;
        private readonly IStockCalculationService _stockCalculationService;

        public DailyTotalsReportService(
            ApplicationDbContext dbContext,
            IDateTimeFilterService dateTimeFilterService,
            IStockCalculationService stockCalculationService)
        {
            _dbContext = dbContext;
            _dateTimeFilterService = dateTimeFilterService;
            _stockCalculationService = stockCalculationService;
        }

        public async Task<List<DailyTotalsDto>> GetDailyTotalsAsync(ProfitLossFilterRequest request, int tenantId)
        {
            var start = DateOnly.FromDateTime(request.StartDate.Date);
            var end = DateOnly.FromDateTime(request.EndDate.Date);
            bool filterByChillar = request.ChillarId.HasValue && request.ChillarId > 0;
            int chillarId = request.ChillarId ?? 0;

            // Calculate previous stock using the helper service
            var previousStock = await _stockCalculationService.CalculatePreviousStockAsync(
                filterByChillar ? chillarId : 0,
                start,
                request.StartTimeOfDay,
                tenantId);

            // Get all data for the period with time filters
            var purchaseGroups = await GetPurchaseTotalsAsync(start, end, tenantId, filterByChillar, chillarId, request.StartTimeOfDay, request.EndTimeOfDay);
            var receiveGroups = await GetReceiveTotalsAsync(start, end, tenantId, filterByChillar, chillarId, request.StartTimeOfDay, request.EndTimeOfDay);
            var salesGroups = await GetSalesTotalsAsync(start, end, tenantId, filterByChillar, chillarId, request.StartTimeOfDay, request.EndTimeOfDay);

            // Combine and calculate final results
            var result = new List<DailyTotalsDto>();
            var runningStock = previousStock;

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var p = purchaseGroups.FirstOrDefault(x => x.Date == d);
                var r = receiveGroups.FirstOrDefault(x => x.Date == d);
                var s = salesGroups.FirstOrDefault(x => x.Date == d);

                var purchaseLiters = p?.TotalPurchaseLiters ?? 0m;
                var purchaseAmount = p?.TotalPurchaseAmount ?? 0m;

                var receiveLiters = r?.TotalReceiveLiters ?? 0m;
                var chillarLoss = r?.ChillarLoss ?? 0m;

                var grossSalesLiters = s?.TotalGrossSalesLiters ?? 0m;
                var salesLiters = s?.TotalSalesLiters ?? 0m;
                var salesAmount = s?.SalesAmount ?? 0m;

                var tsDifference = salesLiters - grossSalesLiters;
                var grossProfit = ReportCalculationHelper.CalculateGrossProfit(salesAmount, purchaseAmount);

                // Calculate daily net balance
                var dailyNetBalance = runningStock + receiveLiters - grossSalesLiters;

                result.Add(new DailyTotalsDto
                {
                    Date = d,
                    TotalPurchaseLiters = purchaseLiters,
                    TotalPurchaseAmount = purchaseAmount,
                    TotalChillarReceiveLiters = receiveLiters,
                    ChillarLoss = chillarLoss,
                    TotalGrossSalesLiters = grossSalesLiters,
                    TotalSalesLiters = salesLiters,
                    SalesAmount = salesAmount,
                    TsDifference = tsDifference,
                    GrossProfit = grossProfit,
                    PreviousStockLiters = runningStock,
                    CurrentStockLiters = dailyNetBalance
                });

                // Update running stock for next day
                runningStock = dailyNetBalance;
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        // Private helpers
        private async Task<List<PurchaseTotalDto>> GetPurchaseTotalsAsync(
            DateOnly start,
            DateOnly end,
            int tenantId,
            bool filterByChillar,
            int? chillarId,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            var purchaseQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= start &&
                            p.Date <= end);

            if (filterByChillar)
                purchaseQuery = purchaseQuery.Where(p => p.Dodhi.ChillarId == chillarId);

            // Apply time filters
            purchaseQuery = _dateTimeFilterService.ApplyPurchaseTimeFilter(purchaseQuery, start, end, startTimeOfDay, endTimeOfDay);

            return await purchaseQuery
                .GroupBy(p => p.Date)
                .Select(g => new PurchaseTotalDto
                {
                    Date = g.Key,
                    TotalPurchaseLiters = g.Sum(p => p.GrossLiters),
                    TotalPurchaseAmount = g.Sum(p => p.GrossLiters * p.Rate)
                })
                .ToListAsync();
        }

        private async Task<List<ReceiveTotalDto>> GetReceiveTotalsAsync(
            DateOnly start,
            DateOnly end,
            int tenantId,
            bool filterByChillar,
            int? chillarId,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            var receiveQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date >= start &&
                            r.Date <= end);

            if (filterByChillar)
                receiveQuery = receiveQuery.Where(r => r.ChillarId == chillarId);

            // Apply time filters
            receiveQuery = _dateTimeFilterService.ApplyChillarReceiveTimeFilter(receiveQuery, start, end, startTimeOfDay, endTimeOfDay);

            return await receiveQuery
                .GroupBy(r => r.Date)
                .Select(g => new ReceiveTotalDto
                {
                    Date = g.Key,
                    TotalReceiveLiters = g.Sum(r => r.GrossLiters),
                    TotalNetLiters = g.Sum(r => r.NetLiters),
                    ChillarLoss = g.Sum(r => (r.GrossLiters - r.NetLiters))
                })
                .ToListAsync();
        }

        private async Task<List<SalesTotalDto>> GetSalesTotalsAsync(
            DateOnly start,
            DateOnly end,
            int tenantId,
            bool filterByChillar,
            int? chillarId,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            var salesQuery = _dbContext.Sales
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= start &&
                            s.Date <= end);

            if (filterByChillar)
                salesQuery = salesQuery.Where(s => s.ChillarId == chillarId);

            // Apply time filters (special logic for Sales)
            salesQuery = _dateTimeFilterService.ApplySalesTimeFilter(salesQuery, start, end, startTimeOfDay, endTimeOfDay);

            return await salesQuery
                .GroupBy(s => s.Date)
                .Select(g => new SalesTotalDto
                {
                    Date = g.Key,
                    TotalGrossSalesLiters = g.Sum(s => s.GrossLiters),
                    TotalSalesLiters = g.Sum(s => s.NetLiters),
                    SalesAmount = g.Sum(s => s.NetLiters * s.Rate)
                })
                .ToListAsync();
        }

        // Internal DTOs
        private class PurchaseTotalDto
        {
            public DateOnly Date { get; set; }
            public decimal TotalPurchaseLiters { get; set; }
            public decimal TotalPurchaseAmount { get; set; }
        }

        private class ReceiveTotalDto
        {
            public DateOnly Date { get; set; }
            public decimal TotalReceiveLiters { get; set; }
            public decimal TotalNetLiters { get; set; }
            public decimal ChillarLoss { get; set; }
        }

        private class SalesTotalDto
        {
            public DateOnly Date { get; set; }
            public decimal TotalGrossSalesLiters { get; set; }
            public decimal TotalSalesLiters { get; set; }
            public decimal SalesAmount { get; set; }
        }
    }
}
