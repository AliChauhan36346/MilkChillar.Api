using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Tenant;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantsService _tenantsService;

        public TenantsController(ITenantsService tenantsService)
        {
            _tenantsService = tenantsService;
        }

        /// <summary>
        /// Create a new tenant with automatic starter accounts setup
        /// </summary>
        /// <param name="request">Tenant creation request</param>
        /// <returns>Tenant setup response with account creation statistics</returns>
        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Name and Email are required" });
            }

            try
            {
                var result = await _tenantsService.CreateTenantAsync(request);

                if (result.SetupSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get tenant by ID
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Tenant details</returns>
        [HttpGet("{tenantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenant(int tenantId)
        {
            if (tenantId <= 0)
            {
                return BadRequest(new { message = "Tenant ID must be greater than 0" });
            }

            var tenant = await _tenantsService.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            return Ok(tenant);
        }

        /// <summary>
        /// Get all tenants
        /// </summary>
        /// <returns>List of all tenants</returns>
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _tenantsService.GetAllTenantsAsync();
            return Ok(tenants);
        }
    }
}
