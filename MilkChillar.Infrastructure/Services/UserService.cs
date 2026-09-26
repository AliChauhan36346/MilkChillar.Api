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
                .Include(u => u.Employee)
                .Include(u => u.Supplier)
                .Include(u => u.Buyer)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IsBlocked = u.IsBlocked,
                    UserType = u.UserType,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    EmployeeId = u.EmployeeId,
                    EmployeeName = u.Employee != null ? u.Employee.FullName : null,
                    SupplierId = u.SupplierId,
                    SupplierName = u.Supplier != null ? u.Supplier.FullName : null,
                    BuyerId = u.BuyerId,
                    BuyerName = u.Buyer != null ? u.Buyer.FullName : null,
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
                var term = query.Search.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.Username.ToLower().Contains(term) ||
                    (u.Employee != null && u.Employee.FullName.ToLower().Contains(term)) ||
                    (u.Supplier != null && u.Supplier.FullName.ToLower().Contains(term)) ||
                    (u.Buyer != null && u.Buyer.FullName.ToLower().Contains(term)));
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
                .Include(u => u.Employee)
                .Include(u => u.Supplier)
                .Include(u => u.Buyer)
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
                    EmployeeId = u.EmployeeId,
                    EmployeeName = u.Employee != null ? u.Employee.FullName : null,
                    SupplierId = u.SupplierId,
                    SupplierName = u.Supplier != null ? u.Supplier.FullName : null,
                    BuyerId = u.BuyerId,
                    BuyerName = u.Buyer != null ? u.Buyer.FullName : null,
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
                .Include(u => u.Employee)
                .Include(u => u.Supplier)
                .Include(u => u.Buyer)
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
                EmployeeId = user.EmployeeId,
                EmployeeName = user.Employee?.FullName,
                SupplierId = user.SupplierId,
                SupplierName = user.Supplier?.FullName,
                BuyerId = user.BuyerId,
                BuyerName = user.Buyer?.FullName,
                CreatedAt = user.CreatedAt,
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, int tenantId)
        {
            var normalizedUsername = dto.Username.Trim();
            var exists = await _context.Users.AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == normalizedUsername.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Username '{normalizedUsername}' already exists.");
            }

            var user = new User
            {
                TenantId = tenantId,
                Username = normalizedUsername,
                PasswordHash = dto.Password,
                IsBlocked = dto.IsBlocked,
                RoleId = dto.RoleId,
                UserType = string.IsNullOrWhiteSpace(dto.UserType) ? "standard" : dto.UserType,
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

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var normalizedUsername = dto.Username.Trim();
                if (!string.Equals(user.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var exists = await _context.Users.AnyAsync(u => u.TenantId == tenantId && u.UserId != id && u.Username.ToLower() == normalizedUsername.ToLower());
                    if (exists)
                    {
                        throw new InvalidOperationException($"Username '{normalizedUsername}' already exists.");
                    }
                    user.Username = normalizedUsername;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = dto.Password;
            }

            user.IsBlocked = dto.IsBlocked;
            if (!string.IsNullOrWhiteSpace(dto.UserType))
            {
                user.UserType = dto.UserType;
            }
            user.RoleId = dto.RoleId;
            user.SupplierId = dto.SupplierId;
            user.EmployeeId = dto.EmployeeId;
            user.BuyerId = dto.BuyerId;

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
