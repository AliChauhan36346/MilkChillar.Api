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

        // Add these endpoints to your ReportsController class

        [HttpGet("AdminDashboardStats")]
        public async Task<IActionResult> GetAdminDashboardStats()
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetAdminDashboardStatsAsync(tenantId);
            return Ok(result);
        }

        [HttpGet("AccountBalances/{accountType}")]
        public async Task<IActionResult> GetAccountBalances(
            string accountType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            try
            {
                var result = await _dashboardStatsService.GetAccountBalancesAsync(
                    accountType, tenantId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("AccountBalances/{accountType}/Summary")]
        public async Task<IActionResult> GetAccountBalanceSummary(string accountType)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            try
            {
                var result = await _dashboardStatsService.GetAccountBalanceSummaryAsync(
                    accountType, tenantId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // Purchase Report Endpoints
        [HttpGet("GetDetailedPurchaseReport")]
        public async Task<IActionResult> GetDetailedPurchaseReport([FromQuery] PurchaseReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetDetailedPurchaseReportAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("GetPurchaseReportSummary")]
        public async Task<IActionResult> GetPurchaseReportSummary([FromQuery] PurchaseReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetPurchaseReportSummaryAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("GetSupplierWisePurchaseReport")]
        public async Task<IActionResult> GetSupplierWisePurchaseReport([FromQuery] PurchaseReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetSupplierWisePurchaseReportAsync(query, tenantId);
            return Ok(result);
        }

        // Sales Report Endpoints
        [HttpGet("GetBuyerWiseSalesReport")]
        public async Task<IActionResult> GetBuyerWiseSalesReport([FromQuery] SalesReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetBuyerWiseSalesReportAsync(query, tenantId);
            return Ok(result);
        }

        [HttpGet("GetSalesReportSummary")]
        public async Task<IActionResult> GetSalesReportSummary([FromQuery] SalesReportQuery query)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var result = await _dashboardStatsService.GetSalesReportSummaryAsync(query, tenantId);
            return Ok(result);
        }

    }

}
