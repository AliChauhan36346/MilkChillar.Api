// Api/Controllers/StockEntryController.cs
using MilkChillar.Application.DTOs.StockEntry;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockEntryController : ControllerBase
    {
        private readonly IStockEntryService _service;

        public StockEntryController(IStockEntryService service)
        {
            _service = service;
        }

        private int GetTenantId() =>
            int.Parse(User.FindFirstValue("tenant_id") ?? throw new UnauthorizedAccessException("Tenant ID missing."));

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID missing."));

        [HttpGet]
        [Authorize(Policy = "stock.read")]
        public async Task<ActionResult<List<StockEntryDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            return Ok(await _service.GetAllAsync(tenantId));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "stock.read")]
        public async Task<ActionResult<StockEntryDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var entry = await _service.GetByIdAsync(id, tenantId);
            if (entry == null) return NotFound();
            return Ok(entry);
        }

        [HttpPost]
        [Authorize(Policy = "stock.create")]
        public async Task<ActionResult<StockEntryDto>> Create([FromBody] CreateStockEntryDto dto)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.StockEntryId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "stock.update")]
        public async Task<ActionResult<StockEntryDto>> Update(int id, [FromBody] UpdateStockEntryDto dto)
        {
            var tenantId = GetTenantId();
            var result = await _service.UpdateAsync(id, dto, tenantId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "stock.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            var success = await _service.DeleteAsync(id, tenantId);
            return success ? NoContent() : NotFound();
        }
    }
}
