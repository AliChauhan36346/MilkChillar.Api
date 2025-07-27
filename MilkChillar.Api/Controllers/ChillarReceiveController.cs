using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MilkChillar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChillarReceiveController : ControllerBase
    {
        private readonly IChillarReceiveService _service;

        public ChillarReceiveController(IChillarReceiveService service)
        {
            _service = service;
        }

        private int GetTenantId()
        {
            var tenantIdStr = User.FindFirstValue("tenant_id");
            if (string.IsNullOrWhiteSpace(tenantIdStr))
                throw new UnauthorizedAccessException("Tenant ID is missing from the token.");

            return int.Parse(tenantIdStr);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found."));

        // GET: api/chillarreceive
        [HttpGet]
        [Authorize(Policy = "chillarreceive.read")]
        public async Task<ActionResult<List<ChillarReceiveDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var result = await _service.GetAllAsync(tenantId);
            return Ok(result);
        }

        // GET: api/chillarreceive/paginated
        [HttpGet("paginated")]
        [Authorize(Policy = "chillarreceive.read")]
        public async Task<ActionResult<PaginatedResult<ChillarReceiveDto>>> GetPaginated([FromQuery] ChillarReceiveQueryParameters parameters)
        {
            parameters.TenantId = GetTenantId();
            var result = await _service.GetPaginatedAsync(parameters);
            return Ok(result);
        }

        // GET: api/chillarreceive/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "chillarreceive.read")]
        public async Task<ActionResult<ChillarReceiveDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var receive = await _service.GetByIdAsync(id, tenantId);
            if (receive == null) return NotFound();
            return Ok(receive);
        }

        // POST: api/chillarreceive
        [HttpPost]
        [Authorize(Policy = "chillarreceive.create")]
        public async Task<ActionResult<ChillarReceiveDto>> Create([FromBody] CreateChillarReceiveDto dto)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();

            var created = await _service.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.ReceiveId }, created);
        }

        // PUT: api/chillarreceive/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "chillarreceive.update")]
        public async Task<ActionResult<ChillarReceiveDto>> Update(int id, [FromBody] UpdateChillarReceiveDto dto)
        {
            var tenantId = GetTenantId();
            var updated = await _service.UpdateAsync(id, dto, tenantId);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: api/chillarreceive/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "chillarreceive.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            var deleted = await _service.DeleteAsync(id, tenantId);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET: api/employees/me
        [HttpGet("/api/employees/me")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentEmployeeChillarInfo()
        {
            var userId = GetUserId();
            var (chillarId, chillarInchargeId) = await _service.GetMyChillarAndInchargeAsync(userId);

            if (chillarId == null || chillarInchargeId == null)
                return NotFound("No chillar or incharge information found for this employee.");

            return Ok(new { ChillarId = chillarId, ChillarInchargeId = chillarInchargeId });
        }

        // GET: api/chillarreceive/metadata?date=2025-07-23&time=morning
        [HttpGet("metadata")]
        [Authorize(Policy = "chillarreceive.read")]
        public async Task<ActionResult<ChillarReceiveMetadataDto>> GetMetadata([FromQuery] DateOnly date, [FromQuery] string time)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var metadata = await _service.GetMetadataAsync(date, time.ToLower(), tenantId, userId);
            return Ok(metadata);
        }

    }
}
