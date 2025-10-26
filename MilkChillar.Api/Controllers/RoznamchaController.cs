using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Roznamcha;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoznamchaController : ControllerBase
    {
        private readonly IRoznamchaService _roznamchaService;

        public RoznamchaController(IRoznamchaService roznamchaService)
        {
            _roznamchaService = roznamchaService;
        }

        /// <summary>
        /// Get all roznamcha entries (Cash & Bank Payments/Receipts)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetRoznamcha([FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetRoznamchaAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve roznamcha entries",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get roznamcha summary only
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetRoznamchaSummary([FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetRoznamchaSummaryAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve roznamcha summary",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get roznamcha entries for a specific account
        /// </summary>
        [HttpGet("by-account/{accountId}")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetRoznamchaByAccount(int accountId, [FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetRoznamchaByAccountAsync(accountId, request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve account roznamcha",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get cash book (all cash transactions)
        /// </summary>
        [HttpGet("cashbook")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetCashBook([FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetCashBookAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve cash book",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bank book (all bank transactions)
        /// </summary>
        [HttpGet("bankbook")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetBankBook([FromQuery] int? bankAccountId, [FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetBankBookAsync(bankAccountId, request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve bank book",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get day book (daily summary of transactions)
        /// </summary>
        [HttpGet("daybook")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetDayBook([FromQuery] RoznamchaFilterRequest request)
        {
            try
            {
                var result = await _roznamchaService.GetDayBookAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve day book",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get list of cash accounts for filter dropdown
        /// </summary>
        [HttpGet("cash-accounts")]
        [Authorize(Policy = "roznamcha.read")]
        public async Task<IActionResult> GetCashAccounts()
        {
            try
            {
                var result = await _roznamchaService.GetCashAccountsAsync();
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve cash accounts",
                    error = ex.Message
                });
            }
        }

        

        /// <summary>
        /// Export roznamcha to Excel/PDF
        /// </summary>
        [HttpGet("export")]
        [Authorize(Policy = "roznamcha.export")]
        public async Task<IActionResult> ExportRoznamcha([FromQuery] RoznamchaFilterRequest request, [FromQuery] string format = "excel")
        {
            try
            {
                var fileBytes = await _roznamchaService.ExportRoznamchaAsync(request, format);
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
                var fileName = $"Roznamcha_{DateTime.Now:yyyyMMddHHmmss}.{format}";

                return File(fileBytes, contentType, fileName);
            }
            catch (NotImplementedException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Export functionality not yet implemented"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to export roznamcha",
                    error = ex.Message
                });
            }
        }
    }
}