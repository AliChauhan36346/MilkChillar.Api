// Api/Controllers/ParchiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Parchi;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParchiController : ControllerBase
    {
        private readonly IParchiService _parchiService;

        public ParchiController(IParchiService parchiService)
        {
            _parchiService = parchiService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
            return int.Parse(tenantIdStr);
        }

        /// <summary>
        /// Get parchi (billing statements) for suppliers within a date range
        /// </summary>
        [HttpGet("suppliers")]
        [Authorize(Policy = "parchi.read")]
        public async Task<IActionResult> GetSupplierParchi([FromQuery] ParchiQueryParameters query)
        {
            if (query.StartDate == default || query.EndDate == default)
            {
                return BadRequest("Start date and end date are required.");
            }

            if (query.EndDate < query.StartDate)
            {
                return BadRequest("End date must be after start date.");
            }

            query.TenantId = GetTenantId();
            var result = await _parchiService.GetSupplierParchiAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get parchi for a single supplier
        /// </summary>
        [HttpGet("suppliers/{supplierId}")]
        [Authorize(Policy = "parchi.read")]
        public async Task<IActionResult> GetSingleSupplierParchi(
            int supplierId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest("Start date and end date are required.");
            }

            var result = await _parchiService.GetSingleSupplierParchiAsync(
                supplierId, startDate, endDate, GetTenantId());

            return result == null ? NotFound() : Ok(result);
        }
    }
}