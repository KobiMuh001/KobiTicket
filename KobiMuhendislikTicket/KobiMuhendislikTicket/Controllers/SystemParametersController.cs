using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KobiMuhendislikTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemParametersController : ControllerBase
    {
        private readonly ISystemParameterService _service;

        public SystemParametersController(ISystemParameterService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("group/{group}")]
        public async Task<IActionResult> GetByGroup(string group)
        {
            var list = await _service.GetByGroupAsync(group);
            return Ok(new { success = true, data = list });
        }

        [AllowAnonymous]
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var list = await _service.GetGroupsAsync();
            return Ok(new { success = true, data = list });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, message = "Bulunamadı." });
            return Ok(new { success = true, data = item });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<IActionResult> Create([FromBody] CreateSystemParameterDto dto)
        {
            var (success, message, id) = await _service.CreateAsync(dto);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message, id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSystemParameterDto dto)
        {
            var (success, message) = await _service.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { success = false, message });
            return Ok(new { success = true, message });
        }
    }
}
