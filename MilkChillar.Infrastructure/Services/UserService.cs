using MilkChillar.Application.DTOs.Users;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync(int tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId)
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IsBlocked = u.IsBlocked,
                    UserType = u.UserType,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParameters query)
        {
            var usersQuery = _context.Users
                .Where(u => u.TenantId == query.TenantId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                usersQuery = usersQuery.Where(u => u.Username.Contains(query.Search));
            }

            if (query.IsBlocked.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsBlocked == query.IsBlocked.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.UserType))
            {
                usersQuery = usersQuery.Where(u => u.UserType == query.UserType);
            }

            if (query.RoleId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == query.RoleId.Value);
            }

            var totalCount = await usersQuery.CountAsync();

            var items = await usersQuery
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IsBlocked = u.IsBlocked,
                    UserType = u.UserType,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync();

            return new PaginatedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<UserDto?> GetByIdAsync(int id, int tenantId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId);

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                IsBlocked = user.IsBlocked,
                UserType = user.UserType,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                CreatedAt = user.CreatedAt,
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, int tenantId)
        {
            var user = new User
            {
                TenantId = tenantId,
                Username = dto.Username,
                PasswordHash = dto.Password, // make sure it's hashed beforehand
                IsBlocked = dto.IsBlocked,
                RoleId = dto.RoleId,
                UserType = dto.UserType,
                SupplierId = dto.SupplierId,
                EmployeeId = dto.EmployeeId,
                BuyerId = dto.BuyerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(user.UserId, tenantId) ?? throw new Exception("User creation failed.");
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, int tenantId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId);
            if (user == null) return null;

            user.IsBlocked = dto.IsBlocked;
            user.UserType = dto.UserType;
            user.RoleId = dto.RoleId;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id, tenantId);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
