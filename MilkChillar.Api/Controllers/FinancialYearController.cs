using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.FinancialYear;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.API.Controllers
{
    [ApiController]
    [Route("api/financial-years")]
    [Authorize]
    public class FinancialYearController : ControllerBase
    {
        private readonly IFinancialYearService _financialYearService;

        public FinancialYearController(IFinancialYearService financialYearService)
        {
            _financialYearService = financialYearService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id") ?? User.FindFirstValue("tenantId");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
            return int.Parse(tenantIdStr);
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId") ?? User.FindFirstValue("id");
            if (string.IsNullOrWhiteSpace(userIdStr))
                throw new UnauthorizedAccessException("User ID is missing from the token.");
            return int.Parse(userIdStr);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tenantId = GetTenantId();
            var list = await _financialYearService.GetAllAsync(tenantId);
            return Ok(list);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var tenantId = GetTenantId();
            var active = await _financialYearService.GetActiveAsync(tenantId);
            if (active == null)
            {
                active = await _financialYearService.EnsureCurrentFinancialYearExistsAsync(tenantId);
            }
            return Ok(active);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tenantId = GetTenantId();
            var fy = await _financialYearService.GetByIdAsync(id, tenantId);
            if (fy == null) return NotFound(new { message = "Financial year not found." });
            return Ok(fy);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFinancialYearDto dto)
        {
            var tenantId = GetTenantId();
            try
            {
                var created = await _financialYearService.CreateAsync(dto, tenantId);
                return CreatedAtAction(nameof(GetById), new { id = created.FinancialYearId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFinancialYearDto dto)
        {
            var tenantId = GetTenantId();
            try
            {
                var updated = await _financialYearService.UpdateAsync(id, dto, tenantId);
                if (updated == null) return NotFound(new { message = "Financial year not found." });
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/set-active")]
        public async Task<IActionResult> SetActive(int id)
        {
            var tenantId = GetTenantId();
            var success = await _financialYearService.SetActiveAsync(id, tenantId);
            if (!success) return NotFound(new { message = "Financial year not found." });
            return Ok(new { message = "Active financial year updated successfully." });
        }

        [HttpGet("{id}/closing-preview")]
        public async Task<IActionResult> GetClosingPreview(int id)
        {
            var tenantId = GetTenantId();
            try
            {
                var preview = await _financialYearService.GetClosingPreviewAsync(id, tenantId);
                return Ok(preview);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseFinancialYear(int id, [FromBody] CloseFinancialYearDto dto)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();

            try
            {
                var result = await _financialYearService.CloseFinancialYearAsync(id, dto, tenantId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/reopen")]
        public async Task<IActionResult> ReopenFinancialYear(int id)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();

            var success = await _financialYearService.ReopenFinancialYearAsync(id, tenantId, userId);
            if (!success)
            {
                return BadRequest(new { message = "Could not reopen financial year. It may not exist or is not closed." });
            }
            return Ok(new { message = "Financial year reopened successfully." });
        }
    }
}
