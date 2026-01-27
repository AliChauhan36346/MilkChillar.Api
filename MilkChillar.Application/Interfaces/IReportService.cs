using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IReportService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardStatsQuery query, int tenantId);
        Task<DodhiPurchaseReportDto> GetDashboardRecordsAsync(DashboardStatsQuery query, int tenantId);
        Task<ChillarInchargeDashboardStatsDto> GetChillarInchargeDashboardStatsAsync(
            ChillarInchargeDashboardQuery query,
            int tenantId
        );
        Task<List<ChillarReceiveDto>> GetChillarReceiveRecordsAsync(
            ChillarInchargeDashboardQuery query,
            int tenantId
        );

        Task<List<SaleDto>> GetSalesReportAsync(SalesReportQuery query, int tenantId); // ✅ New method

        // In Application/Interfaces/IReportService.cs
        Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int tenantId);
        Task<PagedAccountBalancesDto> GetAccountBalancesAsync(string accountType, int tenantId, int pageNumber = 1, int pageSize = 25);
        Task<AccountBalanceSummaryDto> GetAccountBalanceSummaryAsync(string accountType, int tenantId);

        // Purchase Reports
        Task<PagedPurchaseReportDto> GetDetailedPurchaseReportAsync(PurchaseReportQuery query, int tenantId);
        Task<PurchaseReportSummaryDto> GetPurchaseReportSummaryAsync(PurchaseReportQuery query, int tenantId);
        Task<SupplierWisePurchaseReportDto> GetSupplierWisePurchaseReportAsync(PurchaseReportQuery query, int tenantId);

        // Sales Reports
        Task<BuyerWiseSalesReportDto> GetBuyerWiseSalesReportAsync(SalesReportQuery query, int tenantId);
        Task<SalesReportSummaryDto> GetSalesReportSummaryAsync(SalesReportQuery query, int tenantId);

        // NEW - Daily totals over a date range
        Task<List<DailyTotalsDto>> GetDailyTotalsAsync(ProfitLossFilterRequest request, int tenantId);

        // Dodhi Reports
        Task<DodhiWisePurchaseReceiveReportDto> GetDodhiWisePurchaseReceiveReportAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId);

        /// <summary>
        /// Get overall summary for all dodhis in a chillar (total purchase/receive/loss)
        /// Optional: Filter by start and end time of day (morning/evening)
        /// </summary>
        Task<DodhiReportOverallSummaryDto> GetOverallDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId,
            string? startTimeOfDay = null,
            string? endTimeOfDay = null);

        /// <summary>
        /// Get single dodhi-wise summary (individual rows for each dodhi)
        /// Optional: Filter by start and end time of day (morning/evening)
        /// </summary>
        Task<List<DodhiSummaryDto>> GetSingleDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId,
            string? startTimeOfDay = null,
            string? endTimeOfDay = null);

        /// <summary>
        /// Get detailed transaction-wise data for a specific dodhi
        /// </summary>
        Task<DodhiPurchaseReceiveDetailDto> GetDodhiPurchaseReceiveDetailAsync(
            int dodhiId,
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId);
    }
}
