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
                .Where(up => up.User.TenantId == tenantId)
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.Name,
                    GrantedAt = up.GrantedAt
                })
                .ToListAsync();
        }

        public async Task<List<UserPermissionDto>> GetByUserIdAsync(int userId, int tenantId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId && up.User.TenantId == tenantId)
                .Include(up => up.Permission)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.Name,
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId && u.TenantId == tenantId);
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId);
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
                    PermissionName = up.Permission.Name,
                    GrantedAt = up.GrantedAt
                }).ToListAsync();

            return result;
        }
    }
}
