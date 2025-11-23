using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.Responses;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
            return int.Parse(tenantIdStr);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found."));


        //[HttpGet("metadata")]
        //[Authorize(Policy = "purchase.read")]
        //public async Task<ActionResult<PurchaseMetadataDto>> GetPurchaseMetadata([FromQuery] DateOnly date,[FromQuery] string timeOfDay,[FromQuery] int dodhiId)
        //{
        //    // Allow "both" in addition to "morning" and "evening"
        //    if (string.IsNullOrWhiteSpace(timeOfDay) || (timeOfDay != "morning" && timeOfDay != "evening" && timeOfDay != "both"))
        //    {
        //        return BadRequest("TimeOfDay must be 'morning', 'evening', or 'both'");
        //    }

        //    if (dodhiId <= 0)
        //    {
        //        return BadRequest("DodhiId must be a valid positive integer");
        //    }

        //    var tenantId = GetTenantId();
        //    var result = await _purchaseService.GetPurchaseMetadataAsync(date, timeOfDay, tenantId, dodhiId);
        //    return Ok(result);
        //}


        [HttpGet]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult> GetPaginatedPurchases([FromQuery] PurchaseQueryParameters parameters)
        {
            var tenantId = GetTenantId();
            var result = await _purchaseService.GetPaginatedAsync(parameters,tenantId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<PurchaseDto>> GetPurchaseById(int id)
        {
            var tenantId = GetTenantId();
            var result = await _purchaseService.GetByIdAsync(id, tenantId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "purchase.create")]
        public async Task<ActionResult<PurchaseDto>> CreatePurchase([FromBody] CreatePurchaseDto dto)
        {
            var tenantId = GetTenantId();
            var created = await _purchaseService.CreateAsync(dto, tenantId);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "purchase.update")]
        public async Task<ActionResult<PurchaseDto>> UpdatePurchase(int id, [FromBody] UpdatePurchaseDto dto)
        {
            var tenantId = GetTenantId();
            var updated = await _purchaseService.UpdateAsync(id, dto, tenantId);
            return updated is null ? NotFound() : Ok(updated);
        }

        //get my dodhi id
        [HttpGet("mydodhi")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<int>> GetMyDodhiId()
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var dodhiId = await _purchaseService.GetMyDodhiIdAsync(userId);
            return Ok(dodhiId);
        }


        // ============================================
        // Endpoint 1: Get Remaining Suppliers
        // ============================================
        /// <summary>
        /// Get remaining suppliers (not yet added) with pagination and search
        /// </summary>
        /// <param name="date">Date in format YYYY-MM-DD</param>
        /// <param name="timeOfDay">morning, evening, or both</param>
        /// <param name="dodhiId">Dodhi ID</param>
        /// <param name="searchCode">Search by supplier code or name</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 20)</param>
        [HttpGet("remaining-suppliers")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<PaginatedResult<RemainingSupplierDto>>> GetRemainingSuppliers(
            [FromQuery] string date,
            [FromQuery] string timeOfDay = "both",
            [FromQuery] int dodhiId = 0,
            [FromQuery] string? searchCode = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use YYYY-MM-DD");

            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 20;

            var result = await _purchaseService.GetRemainingSuppliers(parsedDate, timeOfDay, dodhiId, searchCode, page, pageSize);
            return Ok(result);
            
            
        }

        // ============================================
        // Endpoint 2: Get Daily Purchases
        // ============================================
        /// <summary>
        /// Get added purchases for a specific date with pagination and search
        /// </summary>
        /// <param name="date">Date in format YYYY-MM-DD</param>
        /// <param name="timeOfDay">morning, evening, or both</param>
        /// <param name="dodhiId">Dodhi ID</param>
        /// <param name="searchCode">Search by supplier code or name</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 20)</param>
        [HttpGet("daily")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<PaginatedResult<PurchaseDto>>> GetDailyPurchases(
            [FromQuery] string date,
            [FromQuery] string timeOfDay = "both",
            [FromQuery] int dodhiId = 0,
            [FromQuery] string? searchCode = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
           
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use YYYY-MM-DD");

            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 20;

            var result = await _purchaseService.GetDailyPurchases(parsedDate, timeOfDay, dodhiId, searchCode, page, pageSize);
            return Ok(result);
            
        }

        // ============================================

        // Endpoint 3: Get Purchase Summary
        // ============================================
        /// <summary>
        /// Get purchase summary totals for a specific date
        /// </summary>
        /// <param name="date">Date in format YYYY-MM-DD</param>
        /// <param name="dodhiId">Dodhi ID</param>
        /// <param name="timeOfDay">Optional filter: morning, evening, or both</param>
        [HttpGet("summary")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<PurchaseSummaryDto>> GetPurchaseSummary(
            [FromQuery] string date,
            [FromQuery] int dodhiId = 0,
            [FromQuery] string? timeOfDay = null)
        {

            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use YYYY-MM-DD");

            var result = await _purchaseService.GetPurchaseSummary(parsedDate, dodhiId, timeOfDay);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "purchase.delete")]
        public async Task<ActionResult> DeletePurchase(int id)
        {
            var tenantId = GetTenantId();
            var deleted = await _purchaseService.DeleteAsync(id, tenantId);
            return deleted ? NoContent() : NotFound();
        }

    }
}