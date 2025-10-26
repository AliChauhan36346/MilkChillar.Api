using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/reports/[controller]")]
    public class ProfitLossController : ControllerBase
    {
        private readonly IProfitLossService _profitLossService;

        public ProfitLossController(IProfitLossService profitLossService)
        {
            _profitLossService = profitLossService;
        }

        /// <summary>
        /// Generate Profit & Loss Report for a specific period
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "profitloss.read")]
        public async Task<IActionResult> GetProfitLossReport([FromQuery] ProfitLossFilterRequest request)
        {
            try
            {
                if (request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate and EndDate are required"
                    });
                }

                if (request.StartDate > request.EndDate)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate cannot be greater than EndDate"
                    });
                }

                var result = await _profitLossService.GetProfitLossReportAsync(request);
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
                    message = "Failed to generate Profit & Loss report",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Generate Comparative Profit & Loss Report for two periods
        /// </summary>
        [HttpGet("comparative")]
        [Authorize(Policy = "profitloss.read")]
        public async Task<IActionResult> GetComparativeProfitLoss([FromQuery] ComparativeProfitLossRequest request)
        {
            try
            {
                if (request.Period1StartDate == default || request.Period1EndDate == default ||
                    request.Period2StartDate == default || request.Period2EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "All date fields are required for comparative report"
                    });
                }

                var result = await _profitLossService.GetComparativeProfitLossAsync(request);
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
                    message = "Failed to generate comparative Profit & Loss report",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed expense breakdown
        /// </summary>
        [HttpGet("expenses")]
        [Authorize(Policy = "profitloss.read")]
        public async Task<IActionResult> GetExpenseBreakdown([FromQuery] ProfitLossFilterRequest request)
        {
            try
            {
                if (request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate and EndDate are required"
                    });
                }

                var result = await _profitLossService.GetExpenseBreakdownAsync(request);
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
                    message = "Failed to retrieve expense breakdown",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed income breakdown
        /// </summary>
        [HttpGet("income")]
        [Authorize(Policy = "profitloss.read")]
        public async Task<IActionResult> GetIncomeBreakdown([FromQuery] ProfitLossFilterRequest request)
        {
            try
            {
                if (request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate and EndDate are required"
                    });
                }

                var result = await _profitLossService.GetIncomeBreakdownAsync(request);
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
                    message = "Failed to retrieve income breakdown",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Export Profit & Loss Report to Excel/PDF
        /// </summary>
        [HttpGet("export")]
        [Authorize(Policy = "profitloss.export")]
        public async Task<IActionResult> ExportProfitLoss([FromQuery] ProfitLossFilterRequest request)
        {
            try
            {
                if (request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate and EndDate are required"
                    });
                }

                var format = request.Format?.ToLower() ?? "excel";
                var fileBytes = await _profitLossService.ExportProfitLossAsync(request, format);

                var contentType = format switch
                {
                    "pdf" => "application/pdf",
                    _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
                var fileName = $"ProfitLoss_{DateTime.Now:yyyyMMddHHmmss}.{format}";

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
                    message = "Failed to export Profit & Loss report",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get quick financial summary (for dashboard)
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "profitloss.read")]
        public async Task<IActionResult> GetFinancialSummary([FromQuery] ProfitLossFilterRequest request)
        {
            try
            {
                if (request.StartDate == default || request.EndDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "StartDate and EndDate are required"
                    });
                }

                var plReport = await _profitLossService.GetProfitLossReportAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        period = plReport.Period,
                        totalRevenue = plReport.Income.TotalIncome,
                        totalExpenses = plReport.TotalExpenses,
                        grossProfit = plReport.GrossProfit,
                        netProfit = plReport.NetProfit,
                        netProfitMargin = plReport.NetProfitMargin,
                        grossProfitMargin = plReport.GrossProfitMargin
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve financial summary",
                    error = ex.Message
                });
            }
        }
    }
}