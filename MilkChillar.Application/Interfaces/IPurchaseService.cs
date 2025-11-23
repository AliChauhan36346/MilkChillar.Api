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

        //Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int userId);

        //Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int dodhiId);

        // get my dodhi id
        Task<int?> GetMyDodhiIdAsync(int userId);

        Task<PaginatedResult<RemainingSupplierDto>> GetRemainingSuppliers(
        DateOnly date,
        string timeOfDay,
        int dodhiId,
        string searchCode,
        int page = 1,
        int pageSize = 20);

        Task<PaginatedResult<PurchaseDto>> GetDailyPurchases(
            DateOnly date,
            string timeOfDay,
            int dodhiId,
            string searchCode,
            int page = 1,
            int pageSize = 20);

        Task<PurchaseSummaryDto> GetPurchaseSummary(
            DateOnly date,
            int dodhiId,
            string? timeOfDay = null);


        Task<bool> DeleteAsync(int purchaseId, int tenantId);

    }
}