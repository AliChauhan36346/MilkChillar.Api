using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.DTOs.Maintenance;

namespace MilkChillar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        private int GetTenantId()
        {
            // Implement tenant ID retrieval logic as per your authentication setup
            return 1;
        }

        [HttpPost("mass-update-supplier-rate-by-dodhi")]
        [Authorize(Policy = "supplier.update")]
        public async Task<IActionResult> MassUpdateSupplierRatesByDodhi(int dodhiId, [FromBody] decimal newRate)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.MassUpdateSupplierRatesByDodhi(dodhiId, newRate, tenantId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("mass-update-supplier-rate")]
        [Authorize(Policy = "supplier.update")]
        public async Task<IActionResult> MassUpdateSupplierRates([FromBody] decimal newRate)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.MassUpdateSupplierRates(newRate, tenantId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("mass-update-buyer-rate")]
        [Authorize(Policy = "buyer.update")]
        public async Task<IActionResult> MassUpdateBuyerRates([FromBody] decimal newRate)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.MassUpdateBuyerRates(newRate, tenantId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("mass-update-supplier-dodhi")]
        [Authorize(Policy = "supplier.update")]
        public async Task<IActionResult> MassUpdateSupplierDodhi(int dodhiId, [FromBody] int[] supplierIds)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.MassUpdateSupplierDodhi(dodhiId, supplierIds, tenantId);
            return result ? Ok() : BadRequest();
        }

        [HttpGet("supplier-rate-summary")]
        [Authorize(Policy = "supplier.read")]
        public async Task<ActionResult<SupplierRateSummaryDto>> GetSupplierRateSummaryForPeriod(int accountId, string startDate, string endDate)
        {
            var tenantId = GetTenantId();
            var summary = await _maintenanceService.GetSupplierRateSummaryForPeriod(
                accountId, DateOnly.Parse(startDate), DateOnly.Parse(endDate), tenantId);
            return Ok(summary);
        }

        [HttpPost("update-supplier-rate-period")]
        [Authorize(Policy = "supplier.update")]
        public async Task<IActionResult> UpdateSupplierRateForPeriod(int accountId, decimal newRate, string startDate, string endDate)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.UpdateSupplierRateForPeriod(
                accountId, newRate, DateOnly.Parse(startDate), DateOnly.Parse(endDate), tenantId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("update-buyer-rate-period")]
        [Authorize(Policy = "buyer.update")]
        public async Task<IActionResult> UpdateBuyerRateForPeriod(int accountId, decimal newRate, string startDate, string endDate)
        {
            var tenantId = GetTenantId();
            var result = await _maintenanceService.UpdateBuyerRateForPeriod(
                accountId, newRate, DateOnly.Parse(startDate), DateOnly.Parse(endDate), tenantId);
            return result ? Ok() : BadRequest();
        }
    }
}
