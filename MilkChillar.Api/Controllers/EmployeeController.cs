using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkChillar.Application.DTOs.Employees;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        [HttpGet("all")]
        [Authorize(Policy = "employee.read")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetAllAsync(GetTenantId());
            return Ok(result);
        }

        [HttpGet("paged")]
        [Authorize(Policy = "employee.read")]
        public async Task<IActionResult> GetPaged([FromQuery] EmployeeQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _employeeService.GetEmployeesAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "employee.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _employeeService.GetByIdAsync(id, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "employee.create")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var result = await _employeeService.CreateAsync(dto, GetTenantId());
            return CreatedAtAction(nameof(GetById), new { id = result.EmployeeId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "employee.update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var result = await _employeeService.UpdateAsync(id, dto, GetTenantId());
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "employee.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _employeeService.DeleteAsync(id, GetTenantId());
            return success ? NoContent() : NotFound();
        }
    }
}
