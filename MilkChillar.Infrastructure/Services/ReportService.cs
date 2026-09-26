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
using MilkChillar.Infrastructure.Services.Reports;

namespace MilkChillar.Infrastructure.Services
{
    /// <summary>
    /// Facade/Orchestrator service that delegates to specialized report services
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly DashboardReportService _dashboardReportService;
        private readonly AccountBalanceReportService _accountBalanceReportService;
        private readonly PurchaseReportService _purchaseReportService;
        private readonly SalesReportService _salesReportService;
        private readonly ChillarReportService _chillarReportService;
        private readonly ProfitLossReportService _profitLossReportService;
        private readonly DailyTotalsReportService _dailyTotalsReportService;
        private readonly DodhiReportService _dodhiReportService;

        public ReportService(
            ApplicationDbContext dbContext,
            IDateTimeFilterService dateTimeFilterService,
            IStockCalculationService stockCalculationService)
        {
            _dashboardReportService = new DashboardReportService(dbContext);
            _accountBalanceReportService = new AccountBalanceReportService(dbContext);
            _purchaseReportService = new PurchaseReportService(dbContext);
            _salesReportService = new SalesReportService(dbContext);
            _chillarReportService = new ChillarReportService(dbContext, dateTimeFilterService, stockCalculationService);
            _profitLossReportService = new ProfitLossReportService(dbContext);
            _dailyTotalsReportService = new DailyTotalsReportService(dbContext, dateTimeFilterService, stockCalculationService);
            _dodhiReportService = new DodhiReportService(dbContext);
        }

        // Dashboard Reports
        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardStatsQuery query, int tenantId)
            => await _chillarReportService.GetDashboardStatsAsync(query, tenantId);

        public async Task<DodhiPurchaseReportDto> GetDashboardRecordsAsync(DashboardStatsQuery query, int tenantId)
            => await _chillarReportService.GetDashboardRecordsAsync(query, tenantId);

        public async Task<ChillarInchargeDashboardStatsDto> GetChillarInchargeDashboardStatsAsync(
            ChillarInchargeDashboardQuery query, int tenantId)
            => await _chillarReportService.GetChillarInchargeDashboardStatsAsync(query, tenantId);

        public async Task<List<ChillarReceiveDto>> GetChillarReceiveRecordsAsync(
            ChillarInchargeDashboardQuery query, int tenantId)
            => await _chillarReportService.GetChillarReceiveRecordsAsync(query, tenantId);

        // Purchase Reports
        public async Task<PagedPurchaseReportDto> GetDetailedPurchaseReportAsync(
            PurchaseReportQuery query, int tenantId)
            => await _purchaseReportService.GetDetailedPurchaseReportAsync(query, tenantId);

        public async Task<PurchaseReportSummaryDto> GetPurchaseReportSummaryAsync(
            PurchaseReportQuery query, int tenantId)
            => await _purchaseReportService.GetPurchaseReportSummaryAsync(query, tenantId);

        public async Task<SupplierWisePurchaseReportDto> GetSupplierWisePurchaseReportAsync(
            PurchaseReportQuery query, int tenantId)
            => await _purchaseReportService.GetSupplierWisePurchaseReportAsync(query, tenantId);

        // Sales Reports
        public async Task<List<SaleDto>> GetSalesReportAsync(SalesReportQuery query, int tenantId)
            => await _salesReportService.GetSalesReportAsync(query, tenantId);

        public async Task<BuyerWiseSalesReportDto> GetBuyerWiseSalesReportAsync(
            SalesReportQuery query, int tenantId)
            => await _salesReportService.GetBuyerWiseSalesReportAsync(query, tenantId);

        public async Task<SalesReportSummaryDto> GetSalesReportSummaryAsync(
            SalesReportQuery query, int tenantId)
            => await _salesReportService.GetSalesReportSummaryAsync(query, tenantId);

        // Account Balance Reports
        public async Task<PagedAccountBalancesDto> GetAccountBalancesAsync(
            string accountType, int tenantId, int pageNumber = 1, int pageSize = 25)
            => await _accountBalanceReportService.GetAccountBalancesAsync(accountType, tenantId, pageNumber, pageSize);

        public async Task<AccountBalanceSummaryDto> GetAccountBalanceSummaryAsync(
            string accountType, int tenantId)
            => await _accountBalanceReportService.GetAccountBalanceSummaryAsync(accountType, tenantId);

        // Dashboard Stats
        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync(int tenantId)
            => await _dashboardReportService.GetAdminDashboardStatsAsync(tenantId);

        // Profit/Loss Reports
        public async Task<List<DailyTotalsDto>> GetDailyTotalsAsync(ProfitLossFilterRequest request, int tenantId)
            => await _dailyTotalsReportService.GetDailyTotalsAsync(request, tenantId);

        // Dodhi Reports
        public async Task<DodhiWisePurchaseReceiveReportDto> GetDodhiWisePurchaseReceiveReportAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
            => await _dodhiReportService.GetDodhiWisePurchaseReceiveReportAsync(chillarId, startDate, endDate, tenantId);

        public async Task<DodhiReportOverallSummaryDto> GetOverallDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId,
            string? startTimeOfDay = null,
            string? endTimeOfDay = null)
            => await _dodhiReportService.GetOverallDodhiSummaryAsync(chillarId, startDate, endDate, tenantId, startTimeOfDay, endTimeOfDay);

        public async Task<List<DodhiSummaryDto>> GetSingleDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId,
            string? startTimeOfDay = null,
            string? endTimeOfDay = null)
            => await _dodhiReportService.GetSingleDodhiSummaryAsync(chillarId, startDate, endDate, tenantId, startTimeOfDay, endTimeOfDay);

        public async Task<DodhiPurchaseReceiveDetailDto> GetDodhiPurchaseReceiveDetailAsync(
            int dodhiId,
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
            => await _dodhiReportService.GetDodhiPurchaseReceiveDetailAsync(dodhiId, chillarId, startDate, endDate, tenantId);
    }
}
