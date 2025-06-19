using MilkChillar.Application.DTOs.Buyers;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IBuyerService
    {
        Task<IEnumerable<BuyerDto>> GetAllAsync(int tenantId);
        Task<BuyerDto?> GetByIdAsync(int id, int tenantId);
        Task<BuyerDto> CreateAsync(CreateBuyerDto dto, int tenantId);
        Task<BuyerDto?> UpdateAsync(int id, UpdateBuyerDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<PaginatedResult<BuyerDto>> GetBuyersAsync(BuyerQueryParameters query);
    }
}
