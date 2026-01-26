using MilkChillar.Application;
using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Parameters;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for sales-related reports
    /// </summary>
    public class SalesReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SaleDto>> GetSalesReportAsync(SalesReportQuery query, int tenantId)
        {
            var sales = BuildSalesQuery(query, tenantId);

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

        public async Task<BuyerWiseSalesReportDto> GetBuyerWiseSalesReportAsync(
            SalesReportQuery query,
            int tenantId)
        {
            var salesQuery = BuildSalesQuery(query, tenantId);

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

            // Calculate rates
            foreach (var buyer in buyerSummaries)
            {
                buyer.AverageRate = ReportCalculationHelper.CalculateAverageRate(buyer.TotalAmount, buyer.TotalNetLiters);
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

            overallSummary.AverageRate = ReportCalculationHelper.CalculateAverageRate(overallSummary.TotalAmount, overallSummary.TotalNetLiters);
            overallSummary.AverageLR = buyerSummaries.Count > 0 ? buyerSummaries.Average(b => b.AverageLR) : 0;
            overallSummary.AverageFat = buyerSummaries.Count > 0 ? buyerSummaries.Average(b => b.AverageFat) : 0;

            return new BuyerWiseSalesReportDto
            {
                BuyerSummaries = buyerSummaries,
                OverallSummary = overallSummary
            };
        }

        public async Task<SalesReportSummaryDto> GetSalesReportSummaryAsync(
            SalesReportQuery query,
            int tenantId)
        {
            var salesQuery = BuildSalesQuery(query, tenantId);

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

            summary.AverageRate = ReportCalculationHelper.CalculateAverageRate(summary.TotalAmount, summary.TotalNetLiters);

            return summary;
        }

        // Private helpers
        private IQueryable<Sales> BuildSalesQuery(SalesReportQuery query, int tenantId)
        {
            var sales = _dbContext.Sales
                .Include(s => s.Account)
                .Where(s => s.TenantId == tenantId &&
                            s.Date >= query.StartDate &&
                            s.Date <= query.EndDate);

            if (query.AccountId.HasValue)
            {
                sales = sales.Where(s => s.AccountId == query.AccountId.Value);
            }

            if (query.ChillarId.HasValue)
            {
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
            }

            return sales;
        }
    }
}
