// Infrastructure/Services/StockEntryService.cs
using MilkChillar.Application.DTOs.StockEntry;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class StockEntryService : IStockEntryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StockEntryService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<StockEntryDto>> GetAllAsync(int tenantId)
        {
            return await _dbContext.StockEntries
                .Include(se => se.Chillar)
                .Where(se => se.TenantId == tenantId)
                .OrderByDescending(se => se.Date)
                .Select(se => new StockEntryDto
                {
                    StockEntryId = se.StockEntryId,
                    Date = se.Date,
                    TimeOfDay = se.TimeOfDay,
                    ChillarName = se.Chillar!.Name,
                    TheoreticalLiters = se.TheoreticalLiters,
                    MeasuredLiters = se.MeasuredLiters,
                    Variance = se.Variance
                })
                .ToListAsync();
        }

        public async Task<StockEntryDto?> GetByIdAsync(int id, int tenantId)
        {
            return await _dbContext.StockEntries
                .Include(se => se.Chillar)
                .Where(se => se.StockEntryId == id && se.TenantId == tenantId)
                .Select(se => new StockEntryDto
                {
                    StockEntryId = se.StockEntryId,
                    Date = se.Date,
                    TimeOfDay = se.TimeOfDay,
                    ChillarName = se.Chillar!.Name,
                    TheoreticalLiters = se.TheoreticalLiters,
                    MeasuredLiters = se.MeasuredLiters,
                    Variance = se.Variance
                })
                .FirstOrDefaultAsync();
        }

        public async Task<StockEntryDto> CreateAsync(CreateStockEntryDto dto, int tenantId, int userId)
        {
            var entity = new StockEntry
            {
                TenantId = tenantId,
                Date = dto.Date,
                TimeOfDay = dto.TimeOfDay,
                ChillarId = dto.ChillarId,
                TheoreticalLiters = dto.TheoreticalLiters,
                MeasuredLiters = dto.MeasuredLiters,
                CreatedBy = userId
            };

            _dbContext.StockEntries.Add(entity);
            await _dbContext.SaveChangesAsync();

            var chillarName = (await _dbContext.Chillars.FindAsync(dto.ChillarId))?.Name ?? "Unknown";

            return new StockEntryDto
            {
                StockEntryId = entity.StockEntryId,
                Date = entity.Date,
                TimeOfDay = entity.TimeOfDay,
                ChillarName = chillarName,
                TheoreticalLiters = entity.TheoreticalLiters,
                MeasuredLiters = entity.MeasuredLiters,
                Variance = entity.MeasuredLiters - entity.TheoreticalLiters
            };
        }

        public async Task<StockEntryDto?> UpdateAsync(int id, UpdateStockEntryDto dto, int tenantId)
        {
            var entity = await _dbContext.StockEntries
                .Include(se => se.Chillar)
                .FirstOrDefaultAsync(se => se.StockEntryId == id && se.TenantId == tenantId);

            if (entity == null)
                return null;

            entity.Date = dto.Date;
            entity.TimeOfDay = dto.TimeOfDay;
            entity.ChillarId = dto.ChillarId;
            entity.TheoreticalLiters = dto.TheoreticalLiters;
            entity.MeasuredLiters = dto.MeasuredLiters;

            await _dbContext.SaveChangesAsync();

            return new StockEntryDto
            {
                StockEntryId = entity.StockEntryId,
                Date = entity.Date,
                TimeOfDay = entity.TimeOfDay,
                ChillarName = entity.Chillar!.Name,
                TheoreticalLiters = entity.TheoreticalLiters,
                MeasuredLiters = entity.MeasuredLiters,
                Variance = entity.Variance
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var entity = await _dbContext.StockEntries
                .FirstOrDefaultAsync(se => se.StockEntryId == id && se.TenantId == tenantId);

            if (entity == null)
                return false;

            _dbContext.StockEntries.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
