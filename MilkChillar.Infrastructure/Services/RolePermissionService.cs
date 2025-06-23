using MilkChillar.Application.DTOs.RolePermissions;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolePermissionDto>> GetAllAsync(int tenantId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.TenantId == tenantId)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Select(rp => new RolePermissionDto
                {
                    TenantId = rp.TenantId,
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    RoleName = rp.Role.Name,
                    PermissionName = rp.Permission.Name
                })
                .ToListAsync();
        }

        public async Task<List<RolePermissionDto>> GetByRoleIdAsync(int roleId, int tenantId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Select(rp => new RolePermissionDto
                {
                    TenantId = rp.TenantId,
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    RoleName = rp.Role.Name,
                    PermissionName = rp.Permission.Name
                })
                .ToListAsync();
        }

        public async Task<RolePermissionDto> CreateAsync(CreateRolePermissionDto dto, int tenantId)
        {
            var exists = await _context.RolePermissions.AnyAsync(rp =>
                rp.TenantId == tenantId &&
                rp.RoleId == dto.RoleId &&
                rp.PermissionId == dto.PermissionId);

            if (exists)
                throw new Exception("RolePermission already exists.");

            var rp = new RolePermission
            {
                TenantId = tenantId,
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId
            };

            _context.RolePermissions.Add(rp);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FindAsync(dto.RoleId);
            var permission = await _context.Permissions.FindAsync(dto.PermissionId);

            return new RolePermissionDto
            {
                TenantId = tenantId,
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId,
                RoleName = role?.Name ?? "",
                PermissionName = permission?.Name ?? ""
            };
        }

        public async Task<bool> DeleteAsync(int roleId, int permissionId)
        {
            var rp = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rp == null)
                return false;

            _context.RolePermissions.Remove(rp);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RolePermissionDto>> AssignMultiplePermissionsAsync(int roleId, List<int> permissionIds, int tenantId)
        {
            var existing = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .ToListAsync();

            var newPermissions = permissionIds
                .Where(pid => !existing.Any(e => e.PermissionId == pid))
                .Select(pid => new RolePermission
                {
                    TenantId = tenantId,
                    RoleId = roleId,
                    PermissionId = pid
                })
                .ToList();

            _context.RolePermissions.AddRange(newPermissions);
            await _context.SaveChangesAsync();

            var result = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Select(rp => new RolePermissionDto
                {
                    TenantId = rp.TenantId,
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    RoleName = rp.Role.Name,
                    PermissionName = rp.Permission.Name
                })
                .ToListAsync();

            return result;
        }
    }
}
