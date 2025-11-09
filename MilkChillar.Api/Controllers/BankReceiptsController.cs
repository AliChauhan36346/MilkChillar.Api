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
    public class BankReceiptsController : ControllerBase
    {
        private readonly IBankTransactionService _bankTransactionService;

        public BankReceiptsController(IBankTransactionService bankTransactionService)
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
        /// Get a paginated and filtered list of bank receipts
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "bankReceipt.read")]
        public async Task<IActionResult> GetPaged([FromQuery] BankTransactionQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _bankTransactionService.GetBankReceiptsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific bank receipt by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "bankReceipt.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bankTransactionService.GetBankReceiptByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get the next receipt number
        /// </summary>
        [HttpGet("next-receipt")]
        [Authorize(Policy = "bankReceipt.create")]
        public async Task<IActionResult> GetNextReceipt()
        {
            var nextReceipt = await _bankTransactionService.GetNextBankReceiptNumberAsync(GetTenantId());
            return Ok(new { nextReceiptNumber = nextReceipt });
        }

        /// <summary>
        /// Create a new bank receipt
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "bankReceipt.create")]
        public async Task<IActionResult> Create([FromBody] CreateBankTransactionDto dto)
        {
            try
            {
                var result = await _bankTransactionService.CreateBankReceiptAsync(dto, GetTenantId(), GetUserId());
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
        /// Update an existing bank receipt
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "bankReceipt.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBankTransactionDto dto)
        {
            try
            {
                var result = await _bankTransactionService.UpdateBankReceiptAsync(id, dto, GetTenantId());
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
        /// Delete a bank receipt by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "bankReceipt.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bankTransactionService.DeleteBankReceiptAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}