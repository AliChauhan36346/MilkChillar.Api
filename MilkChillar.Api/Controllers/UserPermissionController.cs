using MilkChillar.Application.DTOs.UserPermissions;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPermissionController : ControllerBase
    {
        private readonly IUserPermissionService _userPermissionService;

        public UserPermissionController(IUserPermissionService userPermissionService)
        {
            _userPermissionService = userPermissionService;
        }

        private int GetTenantId() =>
            int.TryParse(User.FindFirstValue("tenantId"), out var tenantId) ? tenantId : 0;

        [HttpGet]
        [Authorize(Policy = "userpermission.read")]
        public async Task<IActionResult> GetAll()
        {
            var tenantId = GetTenantId();
            var permissions = await _userPermissionService.GetAllAsync(tenantId);
            return Ok(permissions);
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = "userpermission.read")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var tenantId = GetTenantId();
            var permissions = await _userPermissionService.GetByUserIdAsync(userId, tenantId);
            return Ok(permissions);
        }

        [HttpPost]
        [Authorize(Policy = "userpermission.create")]
        public async Task<IActionResult> Create([FromBody] CreateUserPermissionDto dto)
        {
            var tenantId = GetTenantId();
            var result = await _userPermissionService.CreateAsync(dto, tenantId);
            return Ok(result);
        }

        [HttpPost("assign-multiple")]
        [Authorize(Policy = "userpermission.create")]
        public async Task<IActionResult> AssignMultiple([FromQuery] int userId, [FromBody] List<int> permissionIds)
        {
            var tenantId = GetTenantId();
            var result = await _userPermissionService.AssignMultiplePermissionsAsync(userId, permissionIds, tenantId);
            return Ok(result);
        }

        [HttpDelete("{userId}/{permissionId}")]
        [Authorize(Policy = "userpermission.delete")]
        public async Task<IActionResult> Delete(int userId, int permissionId)
        {
            var result = await _userPermissionService.DeleteAsync(userId, permissionId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
