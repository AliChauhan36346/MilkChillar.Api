using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.DTOs.Purchase;

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
        //public async Task<ActionResult<PurchaseMetadataDto>> GetPurchaseMetadata([FromQuery] DateOnly date, [FromQuery] string timeOfDay)
        //{
        //    // Allow "both" in addition to "morning" and "evening"
        //    if (string.IsNullOrWhiteSpace(timeOfDay) || (timeOfDay != "morning" && timeOfDay != "evening" && timeOfDay != "both"))
        //    {
        //        return BadRequest("TimeOfDay must be 'morning', 'evening', or 'both'");
        //    }

        //    var tenantId = GetTenantId();
        //    var userId = GetUserId(); // Add this line
        //    var result = await _purchaseService.GetPurchaseMetadataAsync(date, timeOfDay, tenantId, userId);
        //    return Ok(result);
        //}


        [HttpGet("metadata")]
        [Authorize(Policy = "purchase.read")]
        public async Task<ActionResult<PurchaseMetadataDto>> GetPurchaseMetadata([FromQuery] DateOnly date,[FromQuery] string timeOfDay,[FromQuery] int dodhiId)
        {
            // Allow "both" in addition to "morning" and "evening"
            if (string.IsNullOrWhiteSpace(timeOfDay) || (timeOfDay != "morning" && timeOfDay != "evening" && timeOfDay != "both"))
            {
                return BadRequest("TimeOfDay must be 'morning', 'evening', or 'both'");
            }

            if (dodhiId <= 0)
            {
                return BadRequest("DodhiId must be a valid positive integer");
            }

            var tenantId = GetTenantId();
            var result = await _purchaseService.GetPurchaseMetadataAsync(date, timeOfDay, tenantId, dodhiId);
            return Ok(result);
        }


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
    }
}