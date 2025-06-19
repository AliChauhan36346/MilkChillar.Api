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
        Task<int> CreateMainAccountAsync(CreateMainAccountRequest request);
        Task<int> CreateSubAccountAsync(CreateSubAccountRequest request);
        Task<int> CreateAccountAsync(CreateAccountRequest request);

        Task<List<MainAccountDto>> GetMainAccountsAsync(int tenantId);
        Task<List<SubAccountDto>> GetSubAccountsAsync(int tenantId, int mainAccountId);
        Task<List<AccountDto>> GetAccountsAsync(int tenantId, int subAccountId);
        Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(int tenantId);

    }
}
