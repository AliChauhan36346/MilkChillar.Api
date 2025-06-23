using MilkChillar.Application.DTOs.RolePermissions;

namespace MilkChillar.Application.Interfaces
{
    public interface IRolePermissionService
    {
        Task<List<RolePermissionDto>> GetAllAsync(int tenantId);
        Task<List<RolePermissionDto>> GetByRoleIdAsync(int roleId, int tenantId);
        Task<RolePermissionDto> CreateAsync(CreateRolePermissionDto dto, int tenantId);
        Task<bool> DeleteAsync(int roleId, int permissionId);
        Task<List<RolePermissionDto>> AssignMultiplePermissionsAsync(int roleId, List<int> permissionIds, int tenantId);
    }
}
