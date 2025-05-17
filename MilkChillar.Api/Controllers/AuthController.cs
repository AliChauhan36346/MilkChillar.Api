using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MilkChillar.Application.Auth;
using MilkChillar.Infrastructure;

namespace MilkChillar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(ApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // MilkChillar.Api/Controllers/AuthController.cs
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return Unauthorized("Invalid username");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized("Invalid password");

        var roles = new List<string> { user.Role?.Name ?? "User" };
        var permissions = user.Role?.RolePermissions
            .Select(rp => rp.Permission.Name)
            .Union(user.UserPermissions.Select(up => up.Permission.Name))
            .ToList() ?? new List<string>();

        var token = _tokenService.GenerateToken(user, roles, permissions);

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Username,
                Role = user.Role?.Name,
                user.TenantId,
                Permissions = permissions
            }
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMyInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.Identity?.Name;
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            userId,
            username,
            role
        });
    }
}
