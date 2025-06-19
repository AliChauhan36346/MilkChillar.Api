using MilkChillar.Application.DTOs.Users;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;

namespace MilkChillar.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync(int tenantId);
        Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParameters query);
        Task<UserDto?> GetByIdAsync(int id, int tenantId);
        Task<UserDto> CreateAsync(CreateUserDto dto, int tenantId);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
    }
}
