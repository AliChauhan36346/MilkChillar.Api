using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;

namespace MilkChillar.Application.Interfaces
{
    public interface IChillarReceiveService
    {
        Task<ChillarReceiveDto?> GetByIdAsync(int id, int tenantId);
        Task<List<ChillarReceiveDto>> GetAllAsync(int tenantId);
        Task<PaginatedResult<ChillarReceiveDto>> GetPaginatedAsync(ChillarReceiveQueryParameters parameters);
        Task<ChillarReceiveDto> CreateAsync(CreateChillarReceiveDto dto, int tenantId, int addedByUserId);
        Task<ChillarReceiveDto?> UpdateAsync(int id, UpdateChillarReceiveDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<(int? ChillarId, int? ChillarInchargeId)> GetMyChillarAndInchargeAsync(int userId);
        Task<ChillarReceiveMetadataDto> GetMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int userId);

    }
}
