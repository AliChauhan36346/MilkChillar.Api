//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using MilkChillar.Application.DTOs.AccountLedger;
//using MilkChillar.Application.Interfaces;
//using MilkChillar.Application.Parameters;
//using System.Security.Claims;

//namespace MilkChillar.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AccountLedgerController : ControllerBase
//    {
//        private readonly IAccountLedgerService _accountLedgerService;

//        public AccountLedgerController(IAccountLedgerService accountLedgerService)
//        {
//            _accountLedgerService = accountLedgerService;
//        }

//        private int GetTenantId()
//        {
//            var tenantIdStr = User.FindFirstValue("tenant_id");
//            if (string.IsNullOrWhiteSpace(tenantIdStr))
//                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");
//            return int.Parse(tenantIdStr);
//        }

//        /// <summary>
//        /// Get ledger entries for a specific account with pagination and filters
//        /// </summary>
//        [HttpGet("account/{accountId}")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetAccountLedger(int accountId, [FromQuery] AccountLedgerQueryParameters query)
//        {
//            query.TenantId = GetTenantId();
//            query.AccountId = accountId;

//            var result = await _accountLedgerService.GetAccountLedgerAsync(query);
//            return Ok(result);
//        }

//        /// <summary>
//        /// Get summary information for a specific account
//        /// </summary>
//        [HttpGet("account/{accountId}/summary")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetAccountLedgerSummary(int accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
//        {
//            var result = await _accountLedgerService.GetAccountLedgerSummaryAsync(accountId, GetTenantId(), fromDate, toDate);
//            return result == null ? NotFound() : Ok(result);
//        }

//        /// <summary>
//        /// Get ledger entries for multiple accounts
//        /// </summary>
//        [HttpGet("multiple")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetMultipleAccountLedger([FromQuery] MultipleAccountLedgerQueryParameters query)
//        {
//            query.TenantId = GetTenantId();
//            var result = await _accountLedgerService.GetMultipleAccountLedgerAsync(query);
//            return Ok(result);
//        }

//        /// <summary>
//        /// Get account balance as of a specific date
//        /// </summary>
//        [HttpGet("account/{accountId}/balance")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetAccountBalance(int accountId, [FromQuery] DateTime? asOfDate)
//        {
//            var balance = await _accountLedgerService.GetAccountBalanceAsync(accountId, GetTenantId(), asOfDate);
//            return Ok(new { AccountId = accountId, Balance = balance, AsOfDate = asOfDate ?? DateTime.Now });
//        }

//        /// <summary>
//        /// Get all account balances for the tenant
//        /// </summary>
//        [HttpGet("balances")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetAllAccountBalances([FromQuery] string? accountCodePrefix)
//        {
//            var result = await _accountLedgerService.GetAllAccountBalancesAsync(GetTenantId(), accountCodePrefix);
//            return Ok(result);
//        }

//        /// <summary>
//        /// Get ledger entries by account code prefix (e.g., all cash accounts starting with "100")
//        /// </summary>
//        [HttpGet("by-code-prefix/{codePrefix}")]
//        [Authorize(Policy = "ledger.read")]
//        public async Task<IActionResult> GetLedgerByAccountCodePrefix(string codePrefix, [FromQuery] MultipleAccountLedgerQueryParameters query)
//        {
//            query.TenantId = GetTenantId();
//            query.AccountCodePrefix = codePrefix;

//            var result = await _accountLedgerService.GetMultipleAccountLedgerAsync(query);
//            return Ok(result);
//        }
//    }
//}


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
    [Authorize]
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
        /// Get account ledger with optional grouping by 15-day periods
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountLedger([FromQuery] AccountLedgerQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _accountLedgerService.GetAccountLedgerAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Get account ledger summary
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountLedgerSummary(
            [FromQuery] int accountId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var summary = await _accountLedgerService.GetAccountLedgerSummaryAsync(accountId, GetTenantId(), fromDate, toDate);

            if (summary == null)
                return NotFound(new { message = "Account not found" });

            return Ok(summary);
        }

        /// <summary>
        /// Get multiple account ledgers
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
        [HttpGet("balance")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAccountBalance(
            [FromQuery] int accountId,
            [FromQuery] DateTime? asOfDate = null)
        {
            var balance = await _accountLedgerService.GetAccountBalanceAsync(accountId, GetTenantId(), asOfDate);
            return Ok(new { accountId, balance, asOfDate = asOfDate ?? DateTime.Now });
        }

        /// <summary>
        /// Get all account balances with optional code prefix filter
        /// </summary>
        [HttpGet("balances")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetAllAccountBalances([FromQuery] string? accountCodePrefix = null)
        {
            var balances = await _accountLedgerService.GetAllAccountBalancesAsync(GetTenantId(), accountCodePrefix);
            return Ok(balances);
        }

        /// <summary>
        /// Get Milk Card - Detailed view of purchases/sales for a 15-day period
        /// </summary>
        [HttpGet("milk-card")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetMilkCard([FromQuery] MilkCardQueryParameters query)
        {
            query.TenantId = GetTenantId();

            if (query.Date == default)
                query.Date = DateTime.Today;

            if (string.IsNullOrWhiteSpace(query.TransactionType))
                query.TransactionType = "Purchase";

            var milkCard = await _accountLedgerService.GetMilkCardAsync(query);

            if (milkCard == null)
                return NotFound(new { message = "No transactions found for the specified period" });

            return Ok(milkCard);
        }

        /// <summary>
        /// Get Milk Card for a specific period (alternative endpoint with explicit dates)
        /// </summary>
        [HttpGet("milk-card-period")]
        [Authorize(Policy = "ledger.read")]
        public async Task<IActionResult> GetMilkCardByPeriod(
            [FromQuery] int accountId,
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] string transactionType = "Purchase")
        {
            // Find a date within the period to use with the standard GetMilkCard method
            var queryDate = periodStart.AddDays((periodEnd - periodStart).Days / 2);

            var query = new MilkCardQueryParameters
            {
                TenantId = GetTenantId(),
                AccountId = accountId,
                Date = queryDate,
                TransactionType = transactionType
            };

            var milkCard = await _accountLedgerService.GetMilkCardAsync(query);

            if (milkCard == null)
                return NotFound(new { message = "No transactions found for the specified period" });

            return Ok(milkCard);
        }
    }
}