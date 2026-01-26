using MilkChillar.Application.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IAccountService
    {
        // Create methods
        Task<int> CreateMainAccountAsync(CreateMainAccountRequest request);
        Task<int> CreateSubAccountAsync(CreateSubAccountRequest request);
        Task<int> CreateAccountAsync(CreateAccountRequest request);

        // Read methods
        Task<List<MainAccountDto>> GetMainAccountsAsync(int tenantId);
        Task<MainAccountDto?> GetMainAccountByIdAsync(int mainAccountId);
        
        Task<List<SubAccountDto>> GetSubAccountsAsync(int tenantId, int mainAccountId);
        Task<SubAccountDto?> GetSubAccountByIdAsync(int subAccountId);
        
        Task<List<AccountDto>> GetAccountsAsync(int tenantId, int subAccountId);
        Task<AccountDto?> GetAccountByIdAsync(int accountId);
        
        Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(int tenantId);
        Task<List<SubAccountDto>> GetSubAccountsByMainAccountCodeAsync(string mainAccountCode);
        Task<List<AccountSearchDto>> SearchAccountsAsync(string query, string? mainAccountCode = null);
        Task<List<AccountSearchDto>> GetAccountsByCodePrefixAsync(string codePrefix);

        // Update methods
        Task UpdateMainAccountAsync(int mainAccountId, CreateMainAccountRequest request);
        Task UpdateSubAccountAsync(int subAccountId, CreateSubAccountRequest request);
        Task UpdateAccountAsync(int accountId, CreateAccountRequest request);

        // Delete methods
        Task DeleteMainAccountAsync(int mainAccountId);
        Task DeleteSubAccountAsync(int subAccountId);
        Task DeleteAccountAsync(int accountId);
    }
}


