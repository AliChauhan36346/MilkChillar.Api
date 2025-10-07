using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.AccountOpeningBalances;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Domain.Entities;
using System.Security.Claims;


namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountOpeningBalancesController : ControllerBase
    {
        private readonly IAccountOpeningBalanceService _accountOpeningBalanceService;

        public AccountOpeningBalancesController(IAccountOpeningBalanceService accountOpeningBalanceService)
        {
            _accountOpeningBalanceService = accountOpeningBalanceService;
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
        /// Get all opening balances for the current tenant
        /// </summary>
        [HttpGet("all")]
        [Authorize(Policy = "openingBalance.read")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _accountOpeningBalanceService.GetAllAsync(GetTenantId());
            return Ok(result);
        }

        /// <summary>
        /// Get a paginated and filtered list of opening balances
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Policy = "openingBalance.read")]
        public async Task<IActionResult> GetPaged([FromQuery] AccountOpeningBalanceQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _accountOpeningBalanceService.GetOpeningBalancesAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific opening balance by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "openingBalance.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _accountOpeningBalanceService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get opening balances for a specific account
        /// </summary>
        [HttpGet("by-account/{accountId}")]
        [Authorize(Policy = "openingBalance.read")]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            var result = await _accountOpeningBalanceService.GetByAccountIdAsync(accountId, GetTenantId());
            return Ok(result);
        }

        /// <summary>
        /// Get opening balance for a specific account and date
        /// </summary>
        [HttpGet("by-account/{accountId}/date/{openingDate}")]
        [Authorize(Policy = "openingBalance.read")]
        public async Task<IActionResult> GetByAccountAndDate(int accountId, DateTime openingDate)
        {
            var result = await _accountOpeningBalanceService.GetByAccountAndDateAsync(accountId, openingDate, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Create a new opening balance
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "openingBalance.create")]
        public async Task<IActionResult> Create([FromBody] CreateAccountOpeningBalanceDto dto)
        {
            try
            {
                var result = await _accountOpeningBalanceService.CreateAsync(dto, GetTenantId(), GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = result.OpeningBalanceId }, result);
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
        /// Update an existing opening balance
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "openingBalance.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountOpeningBalanceDto dto)
        {
            try
            {
                var result = await _accountOpeningBalanceService.UpdateAsync(id, dto, GetTenantId());
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
        /// Delete an opening balance by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "openingBalance.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _accountOpeningBalanceService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}
