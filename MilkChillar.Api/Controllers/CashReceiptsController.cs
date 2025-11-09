using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.CashPayments;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CashReceiptsController : ControllerBase
    {
        private readonly ICashReceiptService _cashReceiptService;

        public CashReceiptsController(ICashReceiptService cashReceiptService)
        {
            _cashReceiptService = cashReceiptService;
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
        /// Get a paginated and filtered list of cash receipts
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "cashReceipt.read")]
        public async Task<IActionResult> GetPaged([FromQuery] CashPaymentQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _cashReceiptService.GetCashReceiptsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific cash receipt by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "cashReceipt.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cashReceiptService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get the next receipt number
        /// </summary>
        [HttpGet("next-receipt")]
        [Authorize(Policy = "cashReceipt.create")]
        public async Task<IActionResult> GetNextReceipt()
        {
            var nextReceipt = await _cashReceiptService.GetNextReceiptNumberAsync(GetTenantId());
            return Ok(new { nextReceiptNumber = nextReceipt });
        }

        /// <summary>
        /// Create a new cash receipt
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "cashReceipt.create")]
        public async Task<IActionResult> Create([FromBody] CreateCashPaymentDto dto)
        {
            try
            {
                var result = await _cashReceiptService.CreateAsync(dto, GetTenantId(), GetUserId());
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
        /// Update an existing cash receipt
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "cashReceipt.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCashPaymentDto dto)
        {
            try
            {
                var result = await _cashReceiptService.UpdateAsync(id, dto, GetTenantId());
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
        /// Delete a cash receipt by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "cashReceipt.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cashReceiptService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}