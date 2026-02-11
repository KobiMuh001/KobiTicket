using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KobiMuhendislikTicket.Application.Common;
using System.Security.Claims;

namespace KobiMuhendislikTicket.WebAPI.Controllers
{
    
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/[controller]")]
    public abstract class AdminBaseController : ControllerBase
    {
        protected string CurrentAdminName => User.Identity?.Name ?? "Sistem";

        protected Guid CurrentAdminId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var guid) ? guid : Guid.Empty;
            }
        }

        protected IActionResult SuccessResponse(string message, object? data = null)
        {
            return Ok(new
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTimeHelper.GetLocalNow()
            });
        }

        protected IActionResult ErrorResponse(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                Success = false,
                Message = message,
                Timestamp = DateTimeHelper.GetLocalNow()
            });
        }
    }
}