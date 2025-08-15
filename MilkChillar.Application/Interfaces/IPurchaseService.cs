using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.Responses;
using MilkChillar.Application.Parameters;

namespace MilkChillar.Application.Interfaces
{
    public interface IPurchaseService
    {
        Task<PaginatedResult<PurchaseDto>> GetPaginatedAsync(PurchaseQueryParameters parameters, int tenantId);
        Task<PurchaseDto?> GetByIdAsync(int purchaseId, int tenantId);
        Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int tenantId);
        Task<PurchaseDto?> UpdateAsync(int purchaseId, UpdatePurchaseDto dto, int tenantId);
        Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int userId);

        // get my dodhi id
        Task<int?> GetMyDodhiIdAsync(int userId);
    }
}