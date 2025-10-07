using MilkChillar.Application.DTOs.AccountLedger;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IAccountLedgerService
    {
        /// <summary>
        /// Get ledger entries for a specific account with pagination
        /// </summary>
        Task<PaginatedResult<AccountLedgerDto>> GetAccountLedgerAsync(AccountLedgerQueryParameters query);

        /// <summary>
        /// Get ledger summary for a specific account
        /// </summary>
        Task<AccountLedgerSummaryDto?> GetAccountLedgerSummaryAsync(int accountId, int tenantId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get ledger entries for multiple accounts
        /// </summary>
        Task<PaginatedResult<AccountLedgerDto>> GetMultipleAccountLedgerAsync(MultipleAccountLedgerQueryParameters query);

        /// <summary>
        /// Get account balance at a specific date
        /// </summary>
        Task<decimal> GetAccountBalanceAsync(int accountId, int tenantId, DateTime? asOfDate = null);

        /// <summary>
        /// Get all accounts with their current balances
        /// </summary>
        Task<IEnumerable<AccountLedgerSummaryDto>> GetAllAccountBalancesAsync(int tenantId, string? accountCodePrefix = null);
    }
}
