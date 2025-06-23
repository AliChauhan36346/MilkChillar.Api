using MilkChillar.Application.DTOs.Roles;
using MilkChillar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Policy = "role.read")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "role.read")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = "role.create")]
        public async Task<IActionResult> Create(CreateRoleDto dto)
        {
            var role = await _roleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "role.update")]
        public async Task<IActionResult> Update(int id, UpdateRoleDto dto)
        {
            var updated = await _roleService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "role.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _roleService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
