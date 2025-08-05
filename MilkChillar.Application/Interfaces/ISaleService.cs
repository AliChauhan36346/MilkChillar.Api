using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.Responses;
using MilkChillar.Application.Parameters;

namespace MilkChillar.Application.Interfaces
{
    public interface ISaleService
    {
        
        Task<PaginatedResult<SaleDto>> GetPaginatedAsync(SaleQueryParameters parameters);
        Task<SaleDto?> GetByIdAsync(int saleId, int tenantId);
        Task<SaleDto> CreateAsync(CreateSaleDto dto, int tenantId, int addedByUserId);
        Task<SaleDto?> UpdateAsync(int saleId, UpdateSaleDto dto, int tenantId);
        Task<SalesMetadataDto> GetSalesMetadataAsync(DateOnly date, int tenantId, int userId);
        
    }
}
