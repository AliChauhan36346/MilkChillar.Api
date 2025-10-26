using MilkChillar.Application.DTOs.Roznamcha;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IRoznamchaService
    {
        /// <summary>
        /// Get all roznamcha entries (payments and receipts)
        /// </summary>
        Task<RoznamchaResponse> GetRoznamchaAsync(RoznamchaFilterRequest request);

        /// <summary>
        /// Get roznamcha summary only (no entries)
        /// </summary>
        Task<RoznamchaSummaryResponse> GetRoznamchaSummaryAsync(RoznamchaFilterRequest request);

        /// <summary>
        /// Get roznamcha entries for a specific account
        /// </summary>
        Task<RoznamchaResponse> GetRoznamchaByAccountAsync(int accountId, RoznamchaFilterRequest request);

        /// <summary>
        /// Get cash book (all cash transactions)
        /// </summary>
        Task<RoznamchaResponse> GetCashBookAsync(RoznamchaFilterRequest request);

        /// <summary>
        /// Get bank book (all bank transactions)
        /// </summary>
        Task<RoznamchaResponse> GetBankBookAsync(int? bankAccountId, RoznamchaFilterRequest request);

        /// <summary>
        /// Get day book (daily summary of transactions)
        /// </summary>
        Task<DayBookResponse> GetDayBookAsync(RoznamchaFilterRequest request);

        /// <summary>
        /// Get list of cash accounts for filter dropdown
        /// </summary>
        Task<List<AccountOptionDto>> GetCashAccountsAsync();


        /// <summary>
        /// Export roznamcha to Excel/PDF
        /// </summary>
        Task<byte[]> ExportRoznamchaAsync(RoznamchaFilterRequest request, string format);
    }
}