using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Suppliers;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        /// <summary>
        /// Get all suppliers for the current tenant
        /// </summary>
        [HttpGet("all")]
        [Authorize(Policy = "supplier.read")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _supplierService.GetAllAsync(GetTenantId());
            return Ok(result);
        }

        /// <summary>
        /// Get a paginated and filtered list of suppliers
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Policy = "supplier.read")]
        public async Task<IActionResult> GetPaged([FromQuery] SupplierQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _supplierService.GetSuppliersAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific supplier by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "supplier.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _supplierService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Create a new supplier
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "supplier.create")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            var result = await _supplierService.CreateAsync(dto, GetTenantId());
            return CreatedAtAction(nameof(GetById), new { id = result.SupplierId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "supplier.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            var result = await _supplierService.UpdateAsync(id, dto, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }


        /// <summary>
        /// Delete a supplier by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "supplier.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _supplierService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}
