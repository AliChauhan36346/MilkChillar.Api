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

        [HttpGet("main/{mainAccountId}")]
        [Authorize(Policy = "mainaccount.read")]
        public async Task<IActionResult> GetMainAccountById(int mainAccountId)
        {
            var result = await _accountService.GetMainAccountByIdAsync(mainAccountId);
            if (result == null)
                return NotFound(new { message = "Main account not found" });
            return Ok(result);
        }

        [HttpGet("sub")]
        [Authorize(Policy = "subaccount.read")]
        public async Task<IActionResult> GetSubAccounts([FromQuery] int tenantId, [FromQuery] int mainAccountId)
        {
            var result = await _accountService.GetSubAccountsAsync(tenantId, mainAccountId);
            return Ok(result);
        }

        [HttpGet("sub/{subAccountId}")]
        [Authorize(Policy = "subaccount.read")]
        public async Task<IActionResult> GetSubAccountById(int subAccountId)
        {
            var result = await _accountService.GetSubAccountByIdAsync(subAccountId);
            if (result == null)
                return NotFound(new { message = "Sub account not found" });
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> GetAccounts([FromQuery] int tenantId, [FromQuery] int subAccountId)
        {
            var result = await _accountService.GetAccountsAsync(tenantId, subAccountId);
            return Ok(result);
        }

        [HttpGet("{accountId}")]
        [Authorize(Policy = "account.read")]
        public async Task<IActionResult> GetAccountById(int accountId)
        {
            var result = await _accountService.GetAccountByIdAsync(accountId);
            if (result == null)
                return NotFound(new { message = "Account not found" });
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

        /// <summary>
        /// Update a Main Account
        /// </summary>
        [HttpPut("main/{mainAccountId}")]
        [Authorize(Policy = "mainaccount.update")]
        public async Task<IActionResult> UpdateMainAccount(int mainAccountId, [FromBody] CreateMainAccountRequest request)
        {
            try
            {
                await _accountService.UpdateMainAccountAsync(mainAccountId, request);
                return Ok(new { message = "Main account updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a Main Account
        /// </summary>
        [HttpDelete("main/{mainAccountId}")]
        [Authorize(Policy = "mainaccount.delete")]
        public async Task<IActionResult> DeleteMainAccount(int mainAccountId)
        {
            try
            {
                await _accountService.DeleteMainAccountAsync(mainAccountId);
                return Ok(new { message = "Main account deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a Sub Account
        /// </summary>
        [HttpPut("sub/{subAccountId}")]
        [Authorize(Policy = "subaccount.update")]
        public async Task<IActionResult> UpdateSubAccount(int subAccountId, [FromBody] CreateSubAccountRequest request)
        {
            try
            {
                await _accountService.UpdateSubAccountAsync(subAccountId, request);
                return Ok(new { message = "Sub account updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a Sub Account
        /// </summary>
        [HttpDelete("sub/{subAccountId}")]
        [Authorize(Policy = "subaccount.delete")]
        public async Task<IActionResult> DeleteSubAccount(int subAccountId)
        {
            try
            {
                await _accountService.DeleteSubAccountAsync(subAccountId);
                return Ok(new { message = "Sub account deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an Account
        /// </summary>
        [HttpPut("{accountId}")]
        [Authorize(Policy = "account.update")]
        public async Task<IActionResult> UpdateAccount(int accountId, [FromBody] CreateAccountRequest request)
        {
            try
            {
                await _accountService.UpdateAccountAsync(accountId, request);
                return Ok(new { message = "Account updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete an Account
        /// </summary>
        [HttpDelete("{accountId}")]
        [Authorize(Policy = "account.delete")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            try
            {
                await _accountService.DeleteAccountAsync(accountId);
                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
