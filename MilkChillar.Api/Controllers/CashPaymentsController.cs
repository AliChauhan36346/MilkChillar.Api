using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.CashPayments;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;
using MilkChillar.Domain.Entities;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CashPaymentsController : ControllerBase
    {
        private readonly ICashPaymentService _cashPaymentService;

        public CashPaymentsController(ICashPaymentService cashPaymentService)
        {
            _cashPaymentService = cashPaymentService;
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


        /// <summary>
        /// Get a paginated and filtered list of cash payments
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "cashPayment.read")]
        public async Task<IActionResult> GetPaged([FromQuery] CashPaymentQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _cashPaymentService.GetCashPaymentsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific cash payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "cashPayment.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cashPaymentService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get the next voucher number
        /// </summary>
        [HttpGet("next-voucher")]
        [Authorize(Policy = "cashPayment.create")]
        public async Task<IActionResult> GetNextVoucher()
        {
            var nextVoucher = await _cashPaymentService.GetNextVoucherNumberAsync(GetTenantId());
            return Ok(new { nextVoucherNumber = nextVoucher });
        }

        /// <summary>
        /// Create a new cash payment
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "cashPayment.create")]
        public async Task<IActionResult> Create([FromBody] CreateCashPaymentDto dto)
        {
            try
            {
                var result = await _cashPaymentService.CreateAsync(dto, GetTenantId(), GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = result.CashPaymentId }, result);
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

        /// <summary>
        /// Update an existing cash payment
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "cashPayment.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCashPaymentDto dto)
        {
            try
            {
                var result = await _cashPaymentService.UpdateAsync(id, dto, GetTenantId());
                return result == null ? NotFound() : Ok(result);
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

        /// <summary>
        /// Delete a cash payment by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "cashPayment.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cashPaymentService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}
