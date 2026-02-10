using KobiMuhendislikTicket.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar (Admin kontrolü kaldırıldı)
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly TicketService _ticketService;
        public DashboardController(TicketService ticketService) => _ticketService = ticketService;

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _ticketService.GetAdminDashboardStatsAsync();
            return Ok(new
            {
                Success = true,
                Message = "İstatistikler başarıyla getirildi.",
                Data = stats,
                Timestamp = DateTime.Now
            });
        }
    }
}