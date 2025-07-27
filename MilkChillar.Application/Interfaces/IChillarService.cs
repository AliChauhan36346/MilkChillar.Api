using MilkChillar.Application.DTOs.Chillar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IChillarService
    {
        Task<List<ChillarDto>> GetAllAsync(int tenantId);
        Task<ChillarDto?> GetByIdAsync(int chillarId, int tenantId);
        Task<ChillarDto> CreateAsync(CreateChillarDto dto, int tenantId);
        Task<ChillarDto?> UpdateAsync(int chillarId, UpdateChillarDto dto, int tenantId);
        Task<bool> DeleteAsync(int chillarId, int tenantId);
    }
}
