using MilkChillar.Application.DTOs.RolePermissions;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionsController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        private int GetTenantId() =>
            int.TryParse(User.FindFirstValue("tenant_id") ?? User.FindFirstValue("tenantId"), out var tenantId) ? tenantId : 0;

        [HttpGet]
        [Authorize(Policy = "rolepermission.read")]
        public async Task<IActionResult> GetAll()
        {
            var tenantId = GetTenantId();
            var result = await _rolePermissionService.GetAllAsync(tenantId);
            return Ok(result);
        }

        [HttpGet("role/{roleId}")]
        [Authorize(Policy = "rolepermission.read")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            var tenantId = GetTenantId();
            var result = await _rolePermissionService.GetByRoleIdAsync(roleId, tenantId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "rolepermission.create")]
        public async Task<IActionResult> Create([FromBody] CreateRolePermissionDto dto)
        {
            var tenantId = GetTenantId();
            var created = await _rolePermissionService.CreateAsync(dto, tenantId);
            return Ok(created);
        }

        [HttpPost("assign-multiple")]
        [Authorize(Policy = "rolepermission.create")]
        public async Task<IActionResult> AssignMultiple(int roleId, [FromBody] List<int> permissionIds)
        {
            var tenantId = GetTenantId();
            var result = await _rolePermissionService.AssignMultiplePermissionsAsync(roleId, permissionIds, tenantId);
            return Ok(result);
        }

        [HttpDelete("{roleId}/{permissionId}")]
        [Authorize(Policy = "rolepermission.delete")]
        public async Task<IActionResult> Delete(int roleId, int permissionId)
        {
            var deleted = await _rolePermissionService.DeleteAsync(roleId, permissionId);
            return deleted ? NoContent() : NotFound();
        }
    }
}
