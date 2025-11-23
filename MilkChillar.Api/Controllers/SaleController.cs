using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MilkChillar.Application.Parameters;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _salesService;

        public SalesController(ISaleService salesService)
        {
            _salesService = salesService;
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

        [HttpGet("metadata")]
        [Authorize(Policy = "sales.read")]
        public async Task<ActionResult<SalesMetadataDto>> GetSalesMetadata([FromQuery] DateOnly date)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _salesService.GetSalesMetadataAsync(date, tenantId, userId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "sales.read")]
        public async Task<ActionResult> GetPaginatedSales([FromQuery] SaleQueryParameters parameters)
        {
            var tenantId = GetTenantId();
            parameters.TenantId = tenantId;
            var result = await _salesService.GetPaginatedAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "sales.read")]
        public async Task<ActionResult<SaleDto>> GetSaleById(int id)
        {
            var tenantId = GetTenantId();
            var result = await _salesService.GetByIdAsync(id, tenantId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "sales.create")]
        public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleDto dto)
        {
            var userId = GetUserId();
            var tenantId = GetTenantId();
            var created = await _salesService.CreateAsync(dto, tenantId, userId);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "sales.update")]
        public async Task<ActionResult<SaleDto>> UpdateSale(int id, [FromBody] UpdateSaleDto dto)
        {
            var tenantId = GetTenantId();
            var updated = await _salesService.UpdateAsync(id, dto, tenantId);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "sales.delete")]
        public async Task<ActionResult> DeleteSale(int id)
        {
            var tenantId = GetTenantId();
            var deleted = await _salesService.DeleteAsync(id, tenantId);
            return deleted ? NoContent() : NotFound();
        }

    }
}
