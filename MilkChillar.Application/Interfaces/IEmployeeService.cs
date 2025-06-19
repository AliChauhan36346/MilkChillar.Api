using MilkChillar.Application.DTOs.Employees;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;

namespace MilkChillar.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync(int tenantId);
        Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(EmployeeQueryParameters parameters);
        Task<EmployeeDto?> GetByIdAsync(int employeeId, int tenantId);
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, int tenantId);
        Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
    }
}
