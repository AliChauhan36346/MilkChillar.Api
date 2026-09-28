using MilkChillar.Application.DTOs.UserPermissions;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly ApplicationDbContext _context;

        public UserPermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserPermissionDto>> GetAllAsync(int tenantId)
        {
            return await _context.UserPermissions
                .Where(up => tenantId == 0 || up.User.TenantId == tenantId)
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission != null ? up.Permission.Name : null,
                    GrantedAt = up.GrantedAt
                })
                .ToListAsync();
        }

        public async Task<List<UserPermissionDto>> GetByUserIdAsync(int userId, int tenantId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId && (tenantId == 0 || up.User.TenantId == tenantId))
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission != null ? up.Permission.Name : null,
                    GrantedAt = up.GrantedAt
                })
                .ToListAsync();
        }

        public async Task<UserPermissionDto> CreateAsync(CreateUserPermissionDto dto, int tenantId)
        {
            var exists = await _context.UserPermissions.AnyAsync(
                up => up.UserId == dto.UserId && up.PermissionId == dto.PermissionId);

            if (exists)
                throw new Exception("Permission already assigned to the user.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId && (tenantId == 0 || u.TenantId == tenantId));
            if (user == null)
                throw new Exception("User not found or doesn't belong to the tenant.");

            var userPermission = new UserPermission
            {
                UserId = dto.UserId,
                PermissionId = dto.PermissionId,
                GrantedAt = DateTime.UtcNow
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();

            var permission = await _context.Permissions.FindAsync(dto.PermissionId);

            return new UserPermissionDto
            {
                UserId = dto.UserId,
                PermissionId = dto.PermissionId,
                PermissionName = permission?.Name,
                GrantedAt = userPermission.GrantedAt
            };
        }

        public async Task<bool> DeleteAsync(int userId, int permissionId)
        {
            var existing = await _context.UserPermissions.FirstOrDefaultAsync(up =>
                up.UserId == userId && up.PermissionId == permissionId);

            if (existing == null)
                return false;

            _context.UserPermissions.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserPermissionDto>> AssignMultiplePermissionsAsync(int userId, List<int> permissionIds, int tenantId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && (tenantId == 0 || u.TenantId == tenantId));
            if (user == null)
                throw new Exception("User not found or doesn't belong to the tenant.");

            var alreadyAssigned = await _context.UserPermissions
                .Where(up => up.UserId == userId && permissionIds.Contains(up.PermissionId))
                .Select(up => up.PermissionId)
                .ToListAsync();

            var newAssignments = permissionIds
                .Where(pid => !alreadyAssigned.Contains(pid))
                .Select(pid => new UserPermission
                {
                    UserId = userId,
                    PermissionId = pid,
                    GrantedAt = DateTime.UtcNow
                }).ToList();

            _context.UserPermissions.AddRange(newAssignments);
            await _context.SaveChangesAsync();

            var result = await _context.UserPermissions
                .Where(up => up.UserId == userId && permissionIds.Contains(up.PermissionId))
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission != null ? up.Permission.Name : null,
                    GrantedAt = up.GrantedAt
                }).ToListAsync();

            return result;
        }

        public async Task<List<UserPermissionDto>> SyncUserPermissionsAsync(int userId, List<int> permissionIds, int tenantId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && (tenantId == 0 || u.TenantId == tenantId));
            if (user == null)
                throw new Exception("User not found or doesn't belong to the tenant.");

            var currentPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var currentIds = currentPermissions.Select(p => p.PermissionId).ToHashSet();
            var targetIds = (permissionIds ?? new List<int>()).Distinct().ToHashSet();

            // To delete
            var toDelete = currentPermissions.Where(cp => !targetIds.Contains(cp.PermissionId)).ToList();
            if (toDelete.Any())
            {
                _context.UserPermissions.RemoveRange(toDelete);
            }

            // To add
            var toAddIds = targetIds.Where(id => !currentIds.Contains(id)).ToList();
            if (toAddIds.Any())
            {
                var newPermissions = toAddIds.Select(pid => new UserPermission
                {
                    UserId = userId,
                    PermissionId = pid,
                    GrantedAt = DateTime.UtcNow
                }).ToList();
                _context.UserPermissions.AddRange(newPermissions);
            }

            await _context.SaveChangesAsync();

            return await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission != null ? up.Permission.Name : null,
                    GrantedAt = up.GrantedAt
                }).ToListAsync();
        }

        public async Task<UserEffectivePermissionsDto> GetEffectivePermissionsAsync(int userId, int tenantId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.UserId == userId && (tenantId == 0 || u.TenantId == tenantId));

            if (user == null)
                throw new Exception("User not found or doesn't belong to the tenant.");

            var rolePermissions = user.Role?.RolePermissions?.ToList() ?? new List<RolePermission>();
            var rolePermissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
            var rolePermissionNames = rolePermissions
                .Where(rp => rp.Permission != null && !string.IsNullOrEmpty(rp.Permission.Name))
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            var userPermissions = user.UserPermissions?.ToList() ?? new List<UserPermission>();
            var directUserPermissionIds = userPermissions.Select(up => up.PermissionId).Distinct().ToList();
            var directUserPermissionNames = userPermissions
                .Where(up => up.Permission != null && !string.IsNullOrEmpty(up.Permission.Name))
                .Select(up => up.Permission.Name)
                .Distinct()
                .ToList();

            var effectiveNames = rolePermissionNames.Union(directUserPermissionNames).Distinct().ToList();

            return new UserEffectivePermissionsDto
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                RolePermissionIds = rolePermissionIds,
                RolePermissionNames = rolePermissionNames,
                DirectUserPermissionIds = directUserPermissionIds,
                DirectUserPermissionNames = directUserPermissionNames,
                EffectivePermissionNames = effectiveNames
            };
        }
    }
}
