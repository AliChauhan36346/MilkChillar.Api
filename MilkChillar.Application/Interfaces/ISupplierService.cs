using MilkChillar.Application.DTOs.Suppliers;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllAsync(int tenantId);
        Task<PaginatedResult<SupplierDto>> GetSuppliersAsync(SupplierQueryParameters query);
        Task<SupplierDto?> GetByIdAsync(int id, int tenantId);
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int tenantId);
        Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);

    }
}
