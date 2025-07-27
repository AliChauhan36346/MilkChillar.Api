using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class ChillarReceiveService : IChillarReceiveService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChillarReceiveService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResult<ChillarReceiveDto>> GetPaginatedAsync(ChillarReceiveQueryParameters parameters)
        {
            var query = _dbContext.ChillarReceives
                .Include(cr => cr.Chillar)
                .Include(cr => cr.ChillarIncharge)
                .Include(cr => cr.Dodhi)
                .Where(cr => cr.TenantId == parameters.TenantId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(cr => cr.Date)
                .ThenByDescending(cr => cr.ReceiveId)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(cr => new ChillarReceiveDto
                {
                    ReceiveId = cr.ReceiveId,
                    Date = DateOnly.FromDateTime(cr.Date),
                    TimeOfDay = cr.TimeOfDay,
                    ChillarName = cr.Chillar.Name,
                    InchargeName = cr.ChillarIncharge.FullName,
                    DodhiName = cr.Dodhi.FullName,
                    GrossLiters = cr.GrossLiters,
                    LR = cr.LR,
                    Fat = cr.Fat,
                    NetLiters = cr.NetLiters        
                })
                .ToListAsync();

            return new PaginatedResult<ChillarReceiveDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<ChillarReceiveDto?> GetByIdAsync(int id, int tenantId)
        {
            var receive = await _dbContext.ChillarReceives
                .Where(cr => cr.ReceiveId == id && cr.TenantId == tenantId)
                .Select(cr => new ChillarReceiveDto
                {
                    ReceiveId = cr.ReceiveId,
                    Date = DateOnly.FromDateTime(cr.Date),
                    TimeOfDay = cr.TimeOfDay,
                    ChillarName = cr.Chillar.Name,
                    InchargeName = cr.ChillarIncharge.FullName,
                    DodhiName = cr.Dodhi.FullName,
                    GrossLiters = cr.GrossLiters,
                    LR = cr.LR,
                    Fat = cr.Fat,
                    NetLiters = cr.NetLiters
                })
                .FirstOrDefaultAsync();

            return receive;
        }

        public async Task<ChillarReceiveDto> CreateAsync(CreateChillarReceiveDto dto, int tenantId, int userId)
        {
            var receive = new ChillarReceive
            {
                TenantId = tenantId,
                Date = dto.Date.ToDateTime(TimeOnly.MinValue),
                TimeOfDay = dto.TimeOfDay,
                ChillarId = dto.ChillarId,
                ChillarInchargeId = dto.ChillarInchargeId,
                DodhiId = dto.DodhiId,
                GrossLiters = dto.GrossLiters,
                LR = dto.LR,
                Fat = dto.Fat,
                NetLiters = dto.NetLiters,
                AddedBy = userId
            };

            _dbContext.ChillarReceives.Add(receive);
            await _dbContext.SaveChangesAsync();

            var chillar = await _dbContext.Chillars.FindAsync(dto.ChillarId);
            var incharge = await _dbContext.Employees.FindAsync(dto.ChillarInchargeId);
            var dodhi = await _dbContext.Employees.FindAsync(dto.DodhiId);

            return new ChillarReceiveDto
            {
                ReceiveId = receive.ReceiveId,
                Date = dto.Date,
                TimeOfDay = dto.TimeOfDay,
                ChillarName = chillar?.Name ?? "Unknown",
                InchargeName = incharge?.FullName ?? "Unknown",
                DodhiName = dodhi?.FullName ?? "Unknown",
                GrossLiters = dto.GrossLiters,
                LR = dto.LR,
                Fat = dto.Fat,
                NetLiters = dto.NetLiters
            };
        }

        public async Task<ChillarReceiveDto?> UpdateAsync(int id, UpdateChillarReceiveDto dto, int tenantId)
        {
            var receive = await _dbContext.ChillarReceives
                .Include(cr => cr.Chillar)
                .Include(cr => cr.ChillarIncharge)
                .Include(cr => cr.Dodhi)
                .FirstOrDefaultAsync(cr => cr.ReceiveId == id && cr.TenantId == tenantId);

            if (receive == null) return null;

            receive.Date = dto.Date.ToDateTime(TimeOnly.MinValue);
            receive.TimeOfDay = dto.TimeOfDay;
            receive.ChillarId = dto.ChillarId;
            receive.ChillarInchargeId = dto.ChillarInchargeId;
            receive.DodhiId = dto.DodhiId;
            receive.GrossLiters = dto.GrossLiters;
            receive.LR = dto.LR;
            receive.Fat = dto.Fat;
            receive.NetLiters = dto.NetLiters;

            await _dbContext.SaveChangesAsync();

            return new ChillarReceiveDto
            {
                ReceiveId = receive.ReceiveId,
                Date = DateOnly.FromDateTime(receive.Date),
                TimeOfDay = receive.TimeOfDay,
                ChillarName = receive.Chillar.Name,
                InchargeName = receive.ChillarIncharge.FullName,
                DodhiName = receive.Dodhi.FullName,
                GrossLiters = receive.GrossLiters,
                LR = receive.LR,
                Fat = receive.Fat,
                NetLiters = receive.NetLiters
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var receive = await _dbContext.ChillarReceives
                .FirstOrDefaultAsync(cr => cr.ReceiveId == id && cr.TenantId == tenantId);

            if (receive == null) return false;

            _dbContext.ChillarReceives.Remove(receive);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChillarReceiveDto>> GetAllAsync(int tenantId)
        {
            var receives = await _dbContext.ChillarReceives
                .Include(cr => cr.Chillar)
                .Include(cr => cr.ChillarIncharge)
                .Include(cr => cr.Dodhi)
                .Where(cr => cr.TenantId == tenantId)
                .OrderByDescending(cr => cr.Date)
                .ThenByDescending(cr => cr.ReceiveId)
                .Select(cr => new ChillarReceiveDto
                {
                    ReceiveId = cr.ReceiveId,
                    Date = DateOnly.FromDateTime(cr.Date),
                    TimeOfDay = cr.TimeOfDay,
                    ChillarName = cr.Chillar.Name,
                    InchargeName = cr.ChillarIncharge.FullName,
                    DodhiName = cr.Dodhi.FullName,
                    GrossLiters = cr.GrossLiters,
                    LR = cr.LR,
                    Fat = cr.Fat,
                    NetLiters = cr.NetLiters
                })
                .ToListAsync();

            return receives;
        }

        public async Task<(int? ChillarId, int? ChillarInchargeId)> GetMyChillarAndInchargeAsync(int userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var employee = user?.Employee;

            if (employee == null || employee.ChillarId == null)
                return (null, null);

            return (employee.ChillarId, employee.EmployeeId);
        }


        public async Task<ChillarReceiveMetadataDto> GetMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int userId)
        {
            var (chillarId, chillarInchargeId) = await GetMyChillarAndInchargeAsync(userId);

            if (chillarId == null || chillarInchargeId == null)
                return new ChillarReceiveMetadataDto();

            // Fetch all entries already made by this incharge for the selected chillar
            var added = await _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date.Date == date.ToDateTime(TimeOnly.MinValue).Date &&
                            r.TimeOfDay.ToLower() == timeOfDay &&
                            r.ChillarId == chillarId &&
                            r.ChillarInchargeId == chillarInchargeId)
                .Include(r => r.Dodhi)
                .Include(r => r.Chillar)
                .Include(r => r.ChillarIncharge)
                .Select(r => new ChillarReceiveDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = DateOnly.FromDateTime(r.Date),
                    TimeOfDay = r.TimeOfDay,
                    ChillarName = r.Chillar.Name,
                    InchargeName = r.ChillarIncharge.FullName,
                    DodhiName = r.Dodhi.FullName,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .ToListAsync();

            // Only return dodhis that are assigned to the same chillar
            var allDodhis = await _dbContext.Employees
                .Where(e => e.TenantId == tenantId &&
                            e.IsActive &&
                            e.Designation.ToLower() == "dodhi" &&
                            e.ChillarId == chillarId)
                .Select(e => new DodhiSimpleDto
                {
                    DodhiId = e.EmployeeId,
                    FullName = e.FullName
                })
                .ToListAsync();

            var addedDodhiIds = added.Select(a => a.DodhiName).ToHashSet();
            var remaining = allDodhis
                .Where(d => !addedDodhiIds.Contains(d.FullName)) // Filter by name since ID may repeat for multiple entries
                .ToList();

            return new ChillarReceiveMetadataDto
            {
                ChillarId = chillarId.Value,
                ChillarInchargeId = chillarInchargeId.Value,
                RemainingDodhis = remaining,
                AddedDodhis = added
            };
        }



    }
}
