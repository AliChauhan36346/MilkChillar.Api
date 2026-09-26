using MilkChillar.Application;
using MilkChillar.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Implementation of stock calculation with optional time filters
    /// Calculates previous stock before a given period starts
    /// </summary>
    public class StockCalculationService : IStockCalculationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeFilterService _dateTimeFilterService;

        public StockCalculationService(ApplicationDbContext dbContext, IDateTimeFilterService dateTimeFilterService)
        {
            _dbContext = dbContext;
            _dateTimeFilterService = dateTimeFilterService;
        }

        public async Task<decimal> CalculatePreviousStockAsync(
            int chillarId,
            DateOnly startDate,
            string? startTimeOfDay,
            int tenantId)
        {
            // Get all receives before the period
            var receiveQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId && r.ChillarId == chillarId && r.Date < startDate);

            // Get all sales before the period
            var salesQuery = _dbContext.Sales
                .Where(s => s.TenantId == tenantId && s.ChillarId == chillarId && s.Date < startDate);

            var totalReceivesBefore = await receiveQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSalesBefore = await salesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;

            var stockBefore = totalReceivesBefore - totalSalesBefore;

            // Apply time filter logic on start date
            if (string.IsNullOrWhiteSpace(startTimeOfDay))
            {
                // No time filter: return stock before start date
                return stockBefore;
            }

            if (startTimeOfDay.ToLower() == "morning")
            {
                // Morning: DO NOT include start date data
                return stockBefore;
            }
            else if (startTimeOfDay.ToLower() == "evening")
            {
                // Evening: Include morning ChillarReceive and Sales of start date
                var morningReceivedOnStartDate = await _dbContext.ChillarReceives
                    .Where(r => r.TenantId == tenantId &&
                                r.ChillarId == chillarId &&
                                r.Date == startDate &&
                                r.TimeOfDay.ToLower() == "morning")
                    .SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;

                var salesOnStartDate = await _dbContext.Sales
                    .Where(s => s.TenantId == tenantId &&
                                s.ChillarId == chillarId &&
                                s.Date == startDate)
                    .SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;

                return stockBefore + morningReceivedOnStartDate + salesOnStartDate;
            }

            return stockBefore;
        }

        public async Task<decimal> CalculatePreviousStockForDodhiAsync(
            int dodhiId,
            int chillarId,
            DateOnly startDate,
            string? startTimeOfDay,
            int tenantId)
        {
            // Get all receives for this dodhi before the period
            var receiveQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.ChillarId == chillarId &&
                            r.DodhiId == dodhiId &&
                            r.Date < startDate);

            // Get all purchases for this dodhi before the period
            var purchaseQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.DodhiId == dodhiId &&
                            p.Date < startDate);

            var totalReceivesBefore = await receiveQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalPurchasesBefore = await purchaseQuery.SumAsync(p => (decimal?)p.GrossLiters) ?? 0m;

            var stockBefore = totalReceivesBefore - totalPurchasesBefore;

            // Apply time filter logic on start date
            if (string.IsNullOrWhiteSpace(startTimeOfDay))
            {
                // No time filter: return stock before start date
                return stockBefore;
            }

            if (startTimeOfDay.ToLower() == "morning")
            {
                // Morning: DO NOT include start date data
                return stockBefore;
            }
            else if (startTimeOfDay.ToLower() == "evening")
            {
                // Evening: Include morning ChillarReceive and Purchases of start date
                var morningReceivedOnStartDate = await _dbContext.ChillarReceives
                    .Where(r => r.TenantId == tenantId &&
                                r.ChillarId == chillarId &&
                                r.DodhiId == dodhiId &&
                                r.Date == startDate &&
                                r.TimeOfDay.ToLower() == "morning")
                    .SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;

                var purchasesOnStartDate = await _dbContext.Purchases
                    .Where(p => p.TenantId == tenantId &&
                                p.DodhiId == dodhiId &&
                                p.Date == startDate)
                    .SumAsync(p => (decimal?)p.GrossLiters) ?? 0m;

                return stockBefore + morningReceivedOnStartDate + purchasesOnStartDate;
            }

            return stockBefore;
        }
    }
}
