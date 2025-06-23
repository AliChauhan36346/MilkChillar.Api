using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilkChillar.Application.DTOs.Roles;

namespace MilkChillar.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(int id);
        Task<RoleDto> CreateAsync(CreateRoleDto dto);
        Task<RoleDto?> UpdateAsync(int id, UpdateRoleDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

