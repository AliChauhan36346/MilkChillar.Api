using MilkChillar.Application.DTOs.AccountOpeningBalances;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;

namespace MilkChillar.Application.Interfaces
{
    public interface IAccountOpeningBalanceService
    {
        Task<IEnumerable<AccountOpeningBalanceDto>> GetAllAsync(int tenantId);
        Task<PaginatedResult<AccountOpeningBalanceDto>> GetOpeningBalancesAsync(AccountOpeningBalanceQueryParameters query);
        Task<AccountOpeningBalanceDto?> GetByIdAsync(int id, int tenantId);
        Task<AccountOpeningBalanceDto> CreateAsync(CreateAccountOpeningBalanceDto dto, int tenantId, int userId);
        Task<AccountOpeningBalanceDto?> UpdateAsync(int id, UpdateAccountOpeningBalanceDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<IEnumerable<AccountOpeningBalanceDto>> GetByAccountIdAsync(int accountId, int tenantId);
        Task<AccountOpeningBalanceDto?> GetByAccountAndDateAsync(int accountId, DateTime openingDate, int tenantId);
    }
}
