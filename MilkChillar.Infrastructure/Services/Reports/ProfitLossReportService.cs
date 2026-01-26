using MilkChillar.Application;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Parameters;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for profit/loss reports
    /// </summary>
    public class ProfitLossReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfitLossReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DailyTotalsDto>> GetDailyTotalsAsync(ProfitLossFilterRequest request, int tenantId)
        {
            var start = DateOnly.FromDateTime(request.StartDate.Date);
            var end = DateOnly.FromDateTime(request.EndDate.Date);
            bool filterByChillar = request.ChillarId > 0;

            // Get all data for the period
            var purchaseGroups = await GetPurchaseTotalsAsync(start, end, tenantId, filterByChillar, request.ChillarId);
            var receiveGroups = await GetReceiveTotalsAsync(start, end, tenantId, filterByChillar, request.ChillarId);
            var salesGroups = await GetSalesTotalsAsync(start, end, tenantId, filterByChillar, request.ChillarId);

            // Combine and calculate final results
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

                var grossSalesLiters = s?.TotalGrossSalesLiters ?? 0m;
                var salesLiters = s?.TotalSalesLiters ?? 0m;
                var salesAmount = s?.SalesAmount ?? 0m;

                var tsDifference = salesLiters - grossSalesLiters;
                var grossProfit = ReportCalculationHelper.CalculateGrossProfit(salesAmount, purchaseAmount);

                result.Add(new DailyTotalsDto
                {
                    Date = d,
                    TotalPurchaseLiters = purchaseLiters,
                    TotalPurchaseAmount = purchaseAmount,
                    TotalChillarReceiveLiters = receiveLiters,
                    DodhiLoss = receiveLiters - purchaseLiters,
                    TotalSalesLiters = grossSalesLiters,
                    ChillarLoss = chillarLoss,
                    TsSalesLiters = salesLiters,
                    TsDifference = tsDifference,
                    SalesAmount = salesAmount,
                    GrossProfit = grossProfit
                });
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        // Private helpers
        private async Task<List<PurchaseTotalDto>> GetPurchaseTotalsAsync(
            DateOnly start,
            DateOnly end,
            int tenantId,
            bool filterByChillar,
            int? chillarId)
        {
            var purchaseQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= start &&
                            p.Date <= end);

            if (filterByChillar)
                purchaseQuery = purchaseQuery.Where(p => p.Dodhi.ChillarId == chillarId);

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
            int? chillarId)
        {
            var receiveQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date >= start &&
                            r.Date <= end);

            if (filterByChillar)
                receiveQuery = receiveQuery.Where(r => r.ChillarId == chillarId);

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
            int? chillarId)
        {
            var salesQuery = _dbContext.Sales
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= start &&
                            s.Date <= end);

            if (filterByChillar)
                salesQuery = salesQuery.Where(s => s.ChillarId == chillarId);

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

        // DTOs for internal use
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
