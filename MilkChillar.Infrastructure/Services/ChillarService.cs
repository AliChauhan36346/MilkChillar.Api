using MilkChillar.Application.DTOs.Chillar;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application.DTOs.Chillar;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class ChillarService : IChillarService
    {
        private readonly ApplicationDbContext _dbContext;

        public ChillarService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ChillarDto>> GetAllAsync(int tenantId)
        {
            return await _dbContext.Chillars
                .Where(c => c.TenantId == tenantId)
                .Select(c => new ChillarDto
                {
                    ChillarId = c.ChillarId,
                    Name = c.Name,
                    Location = c.Location,
                    IsActive = c.IsActive
                }).ToListAsync();
        }

        public async Task<ChillarDto?> GetByIdAsync(int chillarId, int tenantId)
        {
            var chillar = await _dbContext.Chillars
                .FirstOrDefaultAsync(c => c.ChillarId == chillarId && c.TenantId == tenantId);

            if (chillar == null) return null;

            return new ChillarDto
            {
                ChillarId = chillar.ChillarId,
                Name = chillar.Name,
                Location = chillar.Location,
                IsActive = chillar.IsActive
            };
        }

        public async Task<ChillarDto> CreateAsync(CreateChillarDto dto, int tenantId)
        {
            var chillar = new Chillar
            {
                TenantId = tenantId,
                Name = dto.Name,
                Location = dto.Location,
                IsActive = dto.IsActive
            };

            _dbContext.Chillars.Add(chillar);
            await _dbContext.SaveChangesAsync();

            return new ChillarDto
            {
                ChillarId = chillar.ChillarId,
                Name = chillar.Name,
                Location = chillar.Location,
                IsActive = chillar.IsActive
            };
        }

        public async Task<ChillarDto?> UpdateAsync(int chillarId, UpdateChillarDto dto, int tenantId)
        {
            var chillar = await _dbContext.Chillars
                .FirstOrDefaultAsync(c => c.ChillarId == chillarId && c.TenantId == tenantId);

            if (chillar == null) return null;

            chillar.Name = dto.Name;
            chillar.Location = dto.Location;
            chillar.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync();

            return new ChillarDto
            {
                ChillarId = chillar.ChillarId,
                Name = chillar.Name,
                Location = chillar.Location,
                IsActive = chillar.IsActive
            };
        }

        public async Task<bool> DeleteAsync(int chillarId, int tenantId)
        {
            var chillar = await _dbContext.Chillars
                .FirstOrDefaultAsync(c => c.ChillarId == chillarId && c.TenantId == tenantId);

            if (chillar == null) return false;

            _dbContext.Chillars.Remove(chillar);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
