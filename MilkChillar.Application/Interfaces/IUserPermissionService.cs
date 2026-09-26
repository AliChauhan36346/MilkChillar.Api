
using MilkChillar.Application.DTOs.UserPermissions;

namespace MilkChillar.Application.Interfaces
{
    public interface IUserPermissionService
    {
        Task<List<UserPermissionDto>> GetAllAsync(int tenantId);
        Task<List<UserPermissionDto>> GetByUserIdAsync(int userId, int tenantId);
        Task<UserPermissionDto> CreateAsync(CreateUserPermissionDto dto, int tenantId);
        Task<bool> DeleteAsync(int userId, int permissionId);
        Task<List<UserPermissionDto>> AssignMultiplePermissionsAsync(int userId, List<int> permissionIds, int tenantId);
        Task<List<UserPermissionDto>> SyncUserPermissionsAsync(int userId, List<int> permissionIds, int tenantId);
        Task<UserEffectivePermissionsDto> GetEffectivePermissionsAsync(int userId, int tenantId);
    }
}

