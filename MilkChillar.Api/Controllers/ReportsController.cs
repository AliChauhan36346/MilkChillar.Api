using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Infrastructure.Services;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    // Controllers/DashboardStatsController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _dashboardStatsService;

        public ReportsController(IReportService dashboardStatsService)
        {
            _dashboardStatsService = dashboardStatsService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        [HttpGet("DodhiDashboardStats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DashboardStatsQuery query)
        {
            // You would have a proper extension method to extract tenantId from claims
            int tenantId = GetTenantId();
            if (tenantId == 0)
            {
                return Unauthorized("Tenant ID is missing in token.");
            }

            var result = await _dashboardStatsService.GetDashboardStatsAsync(query, tenantId);
            
            
            return Ok(result);
        }

        [HttpGet("GetDodhiPurchaseReport")]
        public async Task<IActionResult> GetDashboardRecords([FromQuery] DashboardStatsQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetDashboardRecordsAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("ChillarInchargeDashboardStats")]
        public async Task<IActionResult> GetChillarInchargeDashboardStats([FromQuery] ChillarInchargeDashboardQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetChillarInchargeDashboardStatsAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("GetChillarReceiveRecords")]
        public async Task<IActionResult> GetChillarReceiveRecords([FromQuery] ChillarInchargeDashboardQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetChillarReceiveRecordsAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("GetSalesReport")]
        public async Task<IActionResult> GetSalesReport([FromQuery] SalesReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetSalesReportAsync(query, tenantId);
            return Ok(result);
        }

    }

}
