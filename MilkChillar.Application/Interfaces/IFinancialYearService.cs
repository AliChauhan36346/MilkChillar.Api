using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MilkChillar.Application.DTOs.FinancialYear;

namespace MilkChillar.Application.Interfaces
{
    public interface IFinancialYearService
    {
        Task<IEnumerable<FinancialYearDto>> GetAllAsync(int tenantId);
        Task<FinancialYearDto?> GetByIdAsync(int id, int tenantId);
        Task<FinancialYearDto?> GetActiveAsync(int tenantId);
        Task<FinancialYearDto> CreateAsync(CreateFinancialYearDto dto, int tenantId);
        Task<FinancialYearDto?> UpdateAsync(int id, UpdateFinancialYearDto dto, int tenantId);
        Task<bool> SetActiveAsync(int id, int tenantId);
        Task<CloseFinancialYearPreviewDto> GetClosingPreviewAsync(int id, int tenantId);
        Task<FinancialYearCloseResultDto> CloseFinancialYearAsync(int id, CloseFinancialYearDto dto, int tenantId, int userId);
        Task<bool> ReopenFinancialYearAsync(int id, int tenantId, int userId);
        Task<FinancialYearDto> EnsureCurrentFinancialYearExistsAsync(int tenantId);
        Task<bool> IsDateInClosedYearAsync(DateTime date, int tenantId);
    }
}
