using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Reports;
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
        public async Task<IActionResult> GetChillarInchargeDashboardStats(
            [FromQuery] int chillarId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            [FromQuery] int? chillarInchargeId = null,
            [FromQuery] int? dodhiId = null,
            [FromQuery] string? startTimeOfDay = null,
            [FromQuery] string? endTimeOfDay = null)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var query = new ChillarInchargeDashboardQuery
            {
                ChillarId = chillarId,
                StartDate = startDate,
                EndDate = endDate,
                ChillarInchargeId = chillarInchargeId,
                DodhiId = dodhiId,
                StartTimeOfDay = startTimeOfDay,
                EndTimeOfDay = endTimeOfDay
            };

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

        // NEW - Daily totals endpoint with optional time filters
        [HttpGet("DailyTotals")]
        public async Task<IActionResult> GetDailyTotals(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int chillarId = 0,
            [FromQuery] string? startTimeOfDay = null,
            [FromQuery] string? endTimeOfDay = null)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            var request = new ProfitLossFilterRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                ChillarId = chillarId,
                StartTimeOfDay = startTimeOfDay,
                EndTimeOfDay = endTimeOfDay,
                Format = "json"
            };

            var result = await _dashboardStatsService.GetDailyTotalsAsync(request, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get overall summary for a chillar (total purchases, receives, losses across all dodhis)
        /// </summary>
        [HttpGet("OverallDodhiSummary")]
        public async Task<IActionResult> GetOverallDodhiSummary(
            [FromQuery] int chillarId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? startTimeOfDay = null,
            [FromQuery] string? endTimeOfDay = null)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            if (chillarId <= 0)
                return BadRequest(new { message = "Chillar ID must be greater than 0" });

            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before end date" });

            try
            {
                var result = await _dashboardStatsService.GetOverallDodhiSummaryAsync(
                    chillarId, startDate, endDate, tenantId, startTimeOfDay, endTimeOfDay);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get single dodhi-wise summary (individual rows for each dodhi with their purchase/receive/loss)
        /// </summary>
        [HttpGet("SingleDodhiSummary")]
        public async Task<IActionResult> GetSingleDodhiSummary(
            [FromQuery] int chillarId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? startTimeOfDay = null,
            [FromQuery] string? endTimeOfDay = null)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            if (chillarId <= 0)
                return BadRequest(new { message = "Chillar ID must be greater than 0" });

            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before end date" });

            try
            {
                var result = await _dashboardStatsService.GetSingleDodhiSummaryAsync(
                    chillarId, startDate, endDate, tenantId, startTimeOfDay, endTimeOfDay);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed transaction-wise data for a specific dodhi (date-wise purchase and receive)
        /// </summary>
        [HttpGet("DodhiPurchaseReceiveDetail")]
        public async Task<IActionResult> GetDodhiPurchaseReceiveDetail(
            [FromQuery] int dodhiId,
            [FromQuery] int chillarId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            int tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized("Tenant ID is missing in token.");

            if (dodhiId <= 0)
                return BadRequest(new { message = "Dodhi ID must be greater than 0" });

            if (chillarId <= 0)
                return BadRequest(new { message = "Chillar ID must be greater than 0" });

            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before end date" });

            try
            {
                var result = await _dashboardStatsService.GetDodhiPurchaseReceiveDetailAsync(
                    dodhiId, chillarId, startDate, endDate, tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }

}


//ok now we have to update the dailytotals report aslo now we have to add the start date time and enddate time the logic will be same for the chillar receive and purchase like we added in dodhi summary and but as in sales we dont have time so when time filter sales criteria will be like that when user we will add sales of the startdate if the startdate time is morning selected but if the startdate time is evening we do not select the sales of that day for the enddate we select sales if the time is morning or evening like in both cases and now we also have to know that how much stock we had before our time filter so we also have to calculate that that will be calculated like that if just date selected we will calculate the chillarreceive - sales before that date and if the time also selected and start time is morning then we do not include the sales and chillarrecive of that day and if the start time is evening then we will include the sales and the morning chillarrecive of that day in stock calculation first tell me you understanding then start updating this
