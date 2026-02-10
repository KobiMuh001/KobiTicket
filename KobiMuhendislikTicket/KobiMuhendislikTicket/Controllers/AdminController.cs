using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly TicketService _ticketService;
        private readonly TenantService _tenantService;
        private readonly StaffService _staffService;
        private readonly NotificationService _notificationService;

        public AdminController(
            AdminService adminService, 
            TicketService ticketService, 
            TenantService tenantService, 
            StaffService staffService,
            NotificationService notificationService)
        {
            _adminService = adminService;
            _ticketService = ticketService;
            _tenantService = tenantService;
            _staffService = staffService;
            _notificationService = notificationService;
        }

#if DEBUG
        [AllowAnonymous]
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var isInAdminRole = User.IsInRole("Admin");
            var roleClaimValue = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                IsInAdminRole = isInAdminRole,
                RoleClaimValue = roleClaimValue,
                AllClaims = claims
            });
        }
#endif

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return Ok(new { success = true, data = dashboard });
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetAllStaff([FromQuery] bool? activeOnly = null)
        {
            var staff = await _staffService.GetAllStaffAsync(activeOnly);
            return Ok(new { success = true, data = staff });
        }

        [HttpGet("staff/{id}")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
                return NotFound(new { success = false, message = "�al��an bulunamad�." });

            return Ok(new { success = true, data = staff });
        }

        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            var result = await _staffService.CreateStaffAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "�al��an olu�turuldu.", data = result.Data });
        }

        [HttpPut("staff/{id}")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffDto dto)
        {
            var result = await _staffService.UpdateStaffAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "�al��an g�ncellendi." });
        }

        [HttpDelete("staff/{id}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            var result = await _staffService.DeleteStaffAsync(id);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "�al��an silindi." });
        }

        [HttpGet("staff/workloads")]
        public async Task<IActionResult> GetStaffWorkloads()
        {
            var workloads = await _staffService.GetStaffWorkloadsAsync();
            return Ok(new { success = true, data = workloads });
        }

        [HttpPost("staff/{id}/reset-password")]
        public async Task<IActionResult> ResetStaffPassword(Guid id, [FromBody] ResetStaffPasswordDto dto)
        {
            var result = await _staffService.ResetStaffPasswordAsync(id, dto.NewPassword);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Şifre başarıyla sıfırlandı." });
        }

        [HttpPost("tickets/{ticketId}/assign-to-staff")]
        public async Task<IActionResult> AssignTicketToStaff(Guid ticketId, [FromBody] AssignTicketToStaffDto dto)
        {
            var result = await _staffService.AssignTicketToStaffAsync(ticketId, dto.StaffId, dto.Note);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket �al��ana atand�." });
        }

        [HttpPost("tickets/bulk-assign")]
        public async Task<IActionResult> BulkAssignTickets([FromBody] BulkAssignDto dto)
        {
            var result = await _staffService.BulkAssignTicketsAsync(dto.TicketIds, dto.StaffId);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = $"{result.Data} ticket ba�ar�yla atand�." });
        }

        [HttpPost("tickets/{ticketId}/auto-assign")]
        public async Task<IActionResult> AutoAssignTicket(Guid ticketId)
        {
            var result = await _staffService.AutoAssignTicketAsync(ticketId);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, data = result.Data });
        }

        [HttpPost("tickets/{ticketId}/unassign")]
        public async Task<IActionResult> UnassignTicket(Guid ticketId)
        {
            var result = await _staffService.UnassignTicketAsync(ticketId);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket atamas� kald�r�ld�." });
        }

        [HttpGet("tenants")]
        public async Task<IActionResult> GetTenants([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            var result = await _adminService.GetTenantsAsync(page, pageSize, search);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("tenants/{id}")]
        public async Task<IActionResult> GetTenantDetail(Guid id)
        {
            var tenant = await _adminService.GetTenantDetailAsync(id);
            if (tenant == null)
                return NotFound(new { success = false, message = "M��teri bulunamad�." });

            return Ok(new { success = true, data = tenant });
        }

        [HttpPut("tenants/{id}")]
        public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto dto)
        {
            var result = await _tenantService.UpdateTenantAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "M��teri bilgileri g�ncellendi." });
        }

        [HttpPost("tenants/{id}/reset-password")]
        public async Task<IActionResult> ResetTenantPassword(Guid id, [FromBody] AdminResetPasswordDto dto)
        {
            var result = await _tenantService.AdminResetPasswordAsync(id, dto.NewPassword);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "M��teri �ifresi ba�ar�yla s�f�rland�." });
        }

        [HttpDelete("tenants/{id}")]
        public async Task<IActionResult> DeleteTenant(Guid id, [FromQuery] bool forceDelete = false)
        {
            var result = await _tenantService.DeleteTenantAsync(id, forceDelete);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, data = result.Data });
        }

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] bool? underWarranty = null)
        {
            var result = await _adminService.GetAssetsAsync(page, pageSize, search, underWarranty);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("assets/{id}")]
        public async Task<IActionResult> GetAssetDetail(Guid id)
        {
            var asset = await _adminService.GetAssetDetailAsync(id);
            if (asset == null)
                return NotFound(new { success = false, message = "Makine bulunamad�." });

            return Ok(new { success = true, data = asset });
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets([FromQuery] TicketFilterDto filter)
        {
            var result = await _adminService.GetTicketsAsync(filter);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicketDetail(Guid id)
        {
            var ticket = await _adminService.GetTicketDetailAsync(id);
            if (ticket == null)
                return NotFound(new { success = false, message = "Ticket bulunamad�." });

            return Ok(new { success = true, data = ticket });
        }

        [HttpPatch("tickets/{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)
        {
            var result = await _ticketService.UpdateTicketStatusAsync(id, dto.NewStatus);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket durumu g�ncellendi." });
        }

        [HttpPatch("tickets/{id}/priority")]
        public async Task<IActionResult> UpdateTicketPriority(Guid id, [FromBody] UpdateTicketPriorityDto dto)
        {
            var result = await _ticketService.UpdateTicketPriorityAsync(id, dto.NewPriority);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket �nceli�i g�ncellendi." });
        }

        [HttpPatch("tickets/{id}/assign")]
        public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketDto dto)
        {
            var result = await _ticketService.AssignTicketToPersonAsync(id, dto.PersonName);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = $"Ticket {dto.PersonName} personeline atand�." });
        }

        [HttpPatch("tickets/{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(Guid id, [FromBody] ResolveTicketDto dto)
        {
            var result = await _ticketService.ResolveTicketAsync(id, dto.SolutionNote, dto.ResolvedBy);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket ��z�ld� olarak i�aretlendi." });
        }

        [HttpPost("tickets/{id}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
        {
            var result = await _ticketService.AddCommentAsync(id, dto.Message, dto.Author, dto.IsAdmin, "Admin");
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            // Müşteri yorumu ise admini bilgilendir
            if (!dto.IsAdmin)
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket.IsSuccess && ticket.Data != null)
                {
                    await _notificationService.NotifyNewCommentAsync(id, ticket.Data.Title, dto.Author, true);
                }
            }

            return Ok(new { success = true, message = "Yorum eklendi." });
        }

        [HttpGet("reports/warranty")]
        public async Task<IActionResult> GetWarrantyReport()
        {
            var report = await _adminService.GetWarrantyReportAsync();
            return Ok(new { success = true, data = report });
        }

        [HttpGet("reports/performance")]
        public async Task<IActionResult> GetPerformanceReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var report = await _adminService.GetPerformanceReportAsync(fromDate, toDate);
            return Ok(new { success = true, data = report });
        }

        // ==================== BİLDİRİMLER ====================

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int take = 20)
        {
            var notifications = await _notificationService.GetAdminNotificationsAsync(take);
            var unreadCount = await _notificationService.GetUnreadCountAsync();
            return Ok(new { success = true, data = notifications, unreadCount });
        }

        [HttpGet("notifications/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            return Ok(new { success = true, count });
        }

        [HttpPatch("notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { success = true, message = "Bildirim okundu olarak işaretlendi." });
        }

        [HttpPatch("notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync();
            return Ok(new { success = true, message = "Tüm bildirimler okundu olarak işaretlendi." });
        }

        [HttpDelete("notifications/{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { success = true, message = "Bildirim silindi." });
        }
    }
}
