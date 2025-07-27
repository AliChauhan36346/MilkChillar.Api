using MilkChillar.Application.DTOs.Employees;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmployeeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EmployeeDto>> GetAllAsync(int tenantId)
        {
            return await _dbContext.Employees
                .Where(e => e.TenantId == tenantId)
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    ContactNumber = e.ContactNumber,
                    Salary = e.Salary,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<EmployeeDto>> GetEmployeesAsync(EmployeeQueryParameters parameters)
        {
            var query = _dbContext.Employees
                .Where(e => e.TenantId == parameters.TenantId);

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(e =>
                    e.FullName.Contains(parameters.Search) ||
                    e.Designation.Contains(parameters.Search));
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(e => e.IsActive == parameters.IsActive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    ContactNumber = e.ContactNumber,
                    Salary = e.Salary,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            return new PaginatedResult<EmployeeDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id, int tenantId)
        {
            var employee = await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id && e.TenantId == tenantId);

            if (employee == null) return null;

            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Designation = employee.Designation,
                ContactNumber = employee.ContactNumber,
                Salary = employee.Salary,
                IsActive = employee.IsActive,
                ChillarId = employee.ChillarId

            };
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, int tenantId)
        {
            var employee = new Employee
            {
                FullName = dto.FullName,
                Designation = dto.Designation,
                ContactNumber = dto.ContactNumber,
                Salary = dto.Salary,
                IsActive = dto.IsActive,
                TenantId = tenantId,
                ChillarId = dto.ChillarId
            };

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Designation = employee.Designation,
                ContactNumber = employee.ContactNumber,
                Salary = employee.Salary,
                IsActive = employee.IsActive,
                ChillarId = employee.ChillarId
            };
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto, int tenantId)
        {
            var employee = await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id && e.TenantId == tenantId);

            if (employee == null) return null;

            employee.FullName = dto.FullName;
            employee.Designation = dto.Designation;
            employee.ContactNumber = dto.ContactNumber;
            employee.Salary = dto.Salary;
            employee.IsActive = dto.IsActive;
            employee.ChillarId = dto.ChillarId;

            await _dbContext.SaveChangesAsync();

            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Designation = employee.Designation,
                ContactNumber = employee.ContactNumber,
                Salary = employee.Salary,
                IsActive = employee.IsActive,
                ChillarId = employee.ChillarId
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var employee = await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id && e.TenantId == tenantId);

            if (employee == null) return false;

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
