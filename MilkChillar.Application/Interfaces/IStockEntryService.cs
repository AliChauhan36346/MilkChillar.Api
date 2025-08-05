using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MilkChillar.Application.DTOs.StockEntry;

namespace MilkChillar.Application.Interfaces
{
    public interface IStockEntryService
    {
        Task<List<StockEntryDto>> GetAllAsync(int tenantId);
        Task<StockEntryDto?> GetByIdAsync(int id, int tenantId);
        Task<StockEntryDto> CreateAsync(CreateStockEntryDto dto, int tenantId, int userId);
        Task<StockEntryDto?> UpdateAsync(int id, UpdateStockEntryDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
    }
}

