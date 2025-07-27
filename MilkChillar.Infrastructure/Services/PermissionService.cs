using MilkChillar.Application.DTOs.Permissions;
using MilkChillar.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermissionDto>> GetAllAsync()
        {
            return await _context.Permissions
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    Name = p.Name,
                    Description = p.Description
                }).ToListAsync();
        }
    }
}
