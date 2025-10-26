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
    }
}
