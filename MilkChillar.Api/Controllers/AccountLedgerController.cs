using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.AccountLedger;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountLedgerController : ControllerBase
    {
        private readonly IAccountLedgerService _accountLedgerService;

        public AccountLedgerController(IAccountLedgerService accountLedgerService)
        {
            _accountLedgerService = accountLedgerService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
            return int.Parse(tenantIdStr);
        }

        /// <summary>
        /// Get ledger entries for a specific account with pagination and filters
        /// </summary>
        [HttpGet("account/{accountId}")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountLedger(int accountId, [FromQuery] AccountLedgerQueryParameters query)
        {
            query.TenantId = GetTenantId();
            query.AccountId = accountId;

            var result = await _accountLedgerService.GetAccountLedgerAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get summary information for a specific account
        /// </summary>
        [HttpGet("account/{accountId}/summary")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountLedgerSummary(int accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _accountLedgerService.GetAccountLedgerSummaryAsync(accountId, GetTenantId(), fromDate, toDate);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Get ledger entries for multiple accounts
        /// </summary>
        [HttpGet("multiple")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetMultipleAccountLedger([FromQuery] MultipleAccountLedgerQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _accountLedgerService.GetMultipleAccountLedgerAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get account balance as of a specific date
        /// </summary>
        [HttpGet("account/{accountId}/balance")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountBalance(int accountId, [FromQuery] DateTime? asOfDate)
        {
            var balance = await _accountLedgerService.GetAccountBalanceAsync(accountId, GetTenantId(), asOfDate);
            return Ok(new { AccountId = accountId, Balance = balance, AsOfDate = asOfDate ?? DateTime.Now });
        }

        /// <summary>
        /// Get all account balances for the tenant
        /// </summary>
        [HttpGet("balances")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAllAccountBalances([FromQuery] string? accountCodePrefix)
        {
            var result = await _accountLedgerService.GetAllAccountBalancesAsync(GetTenantId(), accountCodePrefix);
            return Ok(result);
        }

        /// <summary>
        /// Get ledger entries by account code prefix (e.g., all cash accounts starting with "100")
        /// </summary>
        [HttpGet("by-code-prefix/{codePrefix}")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetLedgerByAccountCodePrefix(string codePrefix, [FromQuery] MultipleAccountLedgerQueryParameters query)
        {
            query.TenantId = GetTenantId();
            query.AccountCodePrefix = codePrefix;

            var result = await _accountLedgerService.GetMultipleAccountLedgerAsync(query);
            return Ok(result);
        }
    }
}