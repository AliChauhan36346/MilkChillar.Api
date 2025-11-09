using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.BankTransactions;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankPaymentsController : ControllerBase
    {
        private readonly IBankTransactionService _bankTransactionService;

        public BankPaymentsController(IBankTransactionService bankTransactionService)
        {
            _bankTransactionService = bankTransactionService;
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
        /// Get a paginated and filtered list of bank payments
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "bankPayment.read")]
        public async Task<IActionResult> GetPaged([FromQuery] BankTransactionQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _bankTransactionService.GetBankPaymentsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific bank payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "bankPayment.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bankTransactionService.GetBankPaymentByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get the next voucher number
        /// </summary>
        [HttpGet("next-voucher")]
        [Authorize(Policy = "bankPayment.create")]
        public async Task<IActionResult> GetNextVoucher()
        {
            var nextVoucher = await _bankTransactionService.GetNextBankPaymentVoucherNumberAsync(GetTenantId());
            return Ok(new { nextVoucherNumber = nextVoucher });
        }

        /// <summary>
        /// Create a new bank payment
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "bankPayment.create")]
        public async Task<IActionResult> Create([FromBody] CreateBankTransactionDto dto)
        {
            try
            {
                var result = await _bankTransactionService.CreateBankPaymentAsync(dto, GetTenantId(), GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = result.TransactionId }, result);
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
        /// Update an existing bank payment
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "bankPayment.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBankTransactionDto dto)
        {
            try
            {
                var result = await _bankTransactionService.UpdateBankPaymentAsync(id, dto, GetTenantId());
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
        /// Delete a bank payment by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "bankPayment.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bankTransactionService.DeleteBankPaymentAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}