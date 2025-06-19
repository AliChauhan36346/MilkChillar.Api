using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Buyers;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuyersController : ControllerBase
    {
        private readonly IBuyerService _buyerService;

        public BuyersController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        /// <summary>
        /// Get all buyers for the current tenant
        /// </summary>
        [HttpGet("all")]
        [Authorize(Policy = "buyer.read")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _buyerService.GetAllAsync(GetTenantId());
            return Ok(result);
        }

        /// <summary>
        /// Get a paginated and filtered list of buyers
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Policy = "buyer.read")]
        public async Task<IActionResult> GetPaged([FromQuery] BuyerQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _buyerService.GetBuyersAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific buyer by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "buyer.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _buyerService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Create a new buyer
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "buyer.create")]
        public async Task<IActionResult> Create([FromBody] CreateBuyerDto dto)
        {
            var result = await _buyerService.CreateAsync(dto, GetTenantId());
            return CreatedAtAction(nameof(GetById), new { id = result.BuyerId }, result);
        }

        /// <summary>
        /// Update an existing buyer
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "buyer.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBuyerDto dto)
        {
            var result = await _buyerService.UpdateAsync(id, dto, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Delete a buyer by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "buyer.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _buyerService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}
