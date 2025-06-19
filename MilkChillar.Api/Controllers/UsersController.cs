using MilkChillar.Application.DTOs.Users;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetTenantId() =>
            int.Parse(User.FindFirstValue("tenantId") ?? "0");

        // GET: api/users
        [HttpGet]
        [Authorize(Policy = "user.read")]
        public async Task<ActionResult<PaginatedResult<UserDto>>> Get([FromQuery] UserQueryParameters query)
        {
            query.TenantId = GetTenantId();
            var result = await _userService.GetUsersAsync(query);
            return Ok(result);
        }

        // GET: api/users/all
        [HttpGet("all")]
        [Authorize(Policy = "user.read")]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var result = await _userService.GetAllAsync(tenantId);
            return Ok(result);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "user.read")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var user = await _userService.GetByIdAsync(id, tenantId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Policy = "user.create")]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
        {
            var tenantId = GetTenantId();
            var created = await _userService.CreateAsync(dto, tenantId);
            return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "user.update")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var tenantId = GetTenantId();
            var updated = await _userService.UpdateAsync(id, dto, tenantId);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "user.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            var success = await _userService.DeleteAsync(id, tenantId);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
