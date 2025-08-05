using MilkChillar.Application.DTOs.Chillar;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChillarController : ControllerBase
    {
        private readonly IChillarService _chillarService;

        public ChillarController(IChillarService chillarService)
        {
            _chillarService = chillarService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        /// <summary>
        /// Get all chillars for the current tenant.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "chillar.read")]
        public async Task<IActionResult> GetAll()
        {
            var tenantId = GetTenantId();
            var chillars = await _chillarService.GetAllAsync(tenantId);
            return Ok(chillars);
        }

        /// <summary>
        /// Get a single chillar by ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "chillar.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var tenantId = GetTenantId();
            var chillar = await _chillarService.GetByIdAsync(id, tenantId);
            return chillar == null ? NotFound() : Ok(chillar);
        }

        /// <summary>
        /// Create a new chillar.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "chillar.create")]
        public async Task<IActionResult> Create([FromBody] CreateChillarDto dto)
        {
            var tenantId = GetTenantId();
            var created = await _chillarService.CreateAsync(dto, tenantId);
            return CreatedAtAction(nameof(GetById), new { id = created.ChillarId }, created);
        }

        /// <summary>
        /// Update an existing chillar.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "chillar.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChillarDto dto)
        {
            var tenantId = GetTenantId();
            var updated = await _chillarService.UpdateAsync(id, dto, tenantId);
            return updated == null ? NotFound() : Ok(updated);
        }

        /// <summary>
        /// Delete a chillar.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "chillar.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            var deleted = await _chillarService.DeleteAsync(id, tenantId);
            return deleted ? NoContent() : NotFound();
        }
    }
}
