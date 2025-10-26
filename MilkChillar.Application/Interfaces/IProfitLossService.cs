using MilkChillar.Application.DTOs.Reports;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    /// <summary>
    /// Service interface for Profit & Loss Report operations
    /// </summary>
    public interface IProfitLossService
    {
        /// <summary>
        /// Generate Profit & Loss report for specified period
        /// </summary>
        /// <param name="request">Filter request with start date and end date</param>
        /// <returns>Complete P&L report with income, expenses, and profit calculations</returns>
        Task<ProfitLossResponse> GetProfitLossReportAsync(ProfitLossFilterRequest request);

        /// <summary>
        /// Generate comparative Profit & Loss report for two periods
        /// </summary>
        /// <param name="request">Request containing date ranges for both periods</param>
        /// <returns>Comparative report showing changes between two periods</returns>
        Task<ProfitLossComparativeResponse> GetComparativeProfitLossAsync(ComparativeProfitLossRequest request);

        /// <summary>
        /// Get detailed expense breakdown by categories
        /// </summary>
        /// <param name="request">Filter request with date range</param>
        /// <returns>Expense breakdown with categories and percentages</returns>
        Task<ExpenseBreakdownResponse> GetExpenseBreakdownAsync(ProfitLossFilterRequest request);

        /// <summary>
        /// Get detailed income breakdown by categories
        /// </summary>
        /// <param name="request">Filter request with date range</param>
        /// <returns>Income breakdown with categories and percentages</returns>
        Task<IncomeBreakdownResponse> GetIncomeBreakdownAsync(ProfitLossFilterRequest request);

        /// <summary>
        /// Export Profit & Loss report in specified format (Excel/PDF)
        /// </summary>
        /// <param name="request">Filter request with date range</param>
        /// <param name="format">Export format: "excel", "pdf", or "csv"</param>
        /// <returns>Byte array of the exported file</returns>
        Task<byte[]> ExportProfitLossAsync(ProfitLossFilterRequest request, string format);
    }
}