using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilkChillar.Application.DTOs.Roles;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetAllAsync()
        {
            return await _context.Roles
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    Name = r.Name,
                    Description = r.Description
                }).ToListAsync();
        }

        public async Task<RoleDto?> GetByIdAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;

            return new RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description
            };
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description
            };
        }

        public async Task<RoleDto?> UpdateAsync(int id, UpdateRoleDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;

            role.Name = dto.Name;
            role.Description = dto.Description;

            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

