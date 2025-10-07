using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Accounts;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Create a new Main Account (e.g., Assets, Liabilities)
        /// </summary>
        [HttpPost("main")]
        [Authorize(Policy = "mainaccount.create")]
        public async Task<IActionResult> CreateMainAccount([FromBody] CreateMainAccountRequest request)
        {
            var id = await _accountService.CreateMainAccountAsync(request);
            return Ok(new { MainAccountId = id });
        }

        /// <summary>
        /// Create a Sub Account under a Main Account
        /// </summary>
        [HttpPost("sub")]
        [Authorize(Policy = "subaccount.create")]
        public async Task<IActionResult> CreateSubAccount([FromBody] CreateSubAccountRequest request)
        {
            var id = await _accountService.CreateSubAccountAsync(request);
            return Ok(new { SubAccountId = id });
        }

        /// <summary>
        /// Create a specific account under a Sub Account (e.g., Bank A, Expense B)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "account.create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var id = await _accountService.CreateAccountAsync(request);
            return Ok(new { AccountId = id });
        }

        [HttpGet("main")]
        [Authorize(Policy = "mainaccount.read")]
        public async Task<IActionResult> GetMainAccounts([FromQuery] int tenantId)
        {
            var result = await _accountService.GetMainAccountsAsync(tenantId);
            return Ok(result);
        }

        [HttpGet("sub")]
        [Authorize(Policy = "subaccount.read")]
        public async Task<IActionResult> GetSubAccounts([FromQuery] int tenantId, [FromQuery] int mainAccountId)
        {
            var result = await _accountService.GetSubAccountsAsync(tenantId, mainAccountId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> GetAccounts([FromQuery] int tenantId, [FromQuery] int subAccountId)
        {
            var result = await _accountService.GetAccountsAsync(tenantId, subAccountId);
            return Ok(result);
        }

        [HttpGet("chart")]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> GetChart([FromQuery] int tenantId)
        {
            var chart = await _accountService.GetChartOfAccountsAsync(tenantId);
            return Ok(chart);
        }

        [HttpGet("sub/by-main-code")]
        [Authorize(Policy = "subaccount.read")]
        public async Task<IActionResult> GetSubAccountsByMainCode([FromQuery] string mainAccountCode)
        {
            var result = await _accountService.GetSubAccountsByMainAccountCodeAsync(mainAccountCode);
            return Ok(result);
        }

        [HttpGet("search")]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> SearchAccounts(
        [FromQuery] string query,
        [FromQuery] string? mainAccountCode = null)
        {
            var result = await _accountService.SearchAccountsAsync(query, mainAccountCode);
            return Ok(result);
        }

        [HttpGet("by-code-prefix")]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> GetAccountsByCodePrefix([FromQuery] string codePrefix)
        {
            try
            {
                var result = await _accountService.GetAccountsByCodePrefixAsync(codePrefix);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
