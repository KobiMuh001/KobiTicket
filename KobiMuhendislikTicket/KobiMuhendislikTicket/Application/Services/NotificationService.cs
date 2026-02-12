using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using KobiMuhendislikTicket.Hubs;
using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace KobiMuhendislikTicket.Application.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub>? _hubContext;
        private readonly IEmailService _emailService;

        public NotificationService(
            ApplicationDbContext context, 
            ILogger<NotificationService> logger,
            IEmailService emailService,
            IHubContext<NotificationHub>? hubContext = null)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        
        public async Task<List<NotificationDto>> GetAdminNotificationsAsync(int take = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => n.IsForAdmin && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    TicketId = n.TicketId,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<List<NotificationDto>> GetStaffNotificationsAsync(int staffId, int take = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsForAdmin && n.TargetUserId == staffId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    TicketId = n.TicketId,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<List<NotificationDto>> GetCustomerNotificationsAsync(int tenantId, int take = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsForAdmin && n.TargetTenantId == tenantId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    TicketId = n.TicketId,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            return notifications;
        }

        
        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.IsForAdmin && !n.IsRead && !n.IsDeleted);
        }

        
        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.UpdatedDate = DateTimeHelper.GetLocalNow();
                await _context.SaveChangesAsync();
            }
        }

        
        public async Task MarkAllAsReadAsync()
        {
            await _context.Notifications
                .Where(n => n.IsForAdmin && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedDate, DateTimeHelper.GetLocalNow()));
        }

        public async Task MarkAllAsReadForStaffAsync(int staffId)
        {
            await _context.Notifications
                .Where(n => !n.IsForAdmin && n.TargetUserId == staffId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedDate, DateTimeHelper.GetLocalNow()));
        }

        public async Task MarkAllAsReadForCustomerAsync(int tenantId)
        {
            await _context.Notifications
                .Where(n => !n.IsForAdmin && n.TargetTenantId == tenantId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedDate, DateTimeHelper.GetLocalNow()));
        }

        public async Task CreateCustomerNotificationAsync(int tenantId, string title, string message, NotificationType type, int? ticketId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                TicketId = ticketId,
                IsForAdmin = false,
                TargetTenantId = tenantId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Müşteriye bildirim gönderildi: {TenantId}, {Title}", tenantId, title);

            if (_hubContext != null)
            {
                try
                {
                    var notificationDto = new NotificationDto
                    {
                        Id = notification.Id,
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        IsRead = false,
                        TicketId = ticketId,
                        CreatedDate = notification.CreatedDate
                    };

                    await _hubContext.Clients.User(tenantId.ToString())
                        .SendAsync("CustomerNotificationReceived", notificationDto)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Müşteriye SignalR bildirim gönderilemedi: {TenantId}", tenantId);
                }
            }
        }

        public async Task NotifyCustomerStatusChangedAsync(int tenantId, int ticketId, string ticketTitle, string newStatus)
        {
            await CreateCustomerNotificationAsync(
                tenantId,
                "Durum Değişikliği",
                $"Ticket durumu değişti: {ticketTitle} → {newStatus}",
                NotificationType.TicketStatusChanged,
                ticketId
            );
        }

        public async Task NotifyCustomerNewCommentAsync(int tenantId, int ticketId, string ticketTitle, string authorName)
        {
            await CreateCustomerNotificationAsync(
                tenantId,
                "Yeni Mesaj",
                $"{authorName} bir mesaj gönderdi: {ticketTitle}",
                NotificationType.TicketComment,
                ticketId
            );
        }

        
        public async Task CreateNotificationAsync(string title, string message, NotificationType type, int? ticketId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                TicketId = ticketId,
                IsForAdmin = true,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Bildirim oluşturuldu: {Title}", title);

            // Admin'lere SignalR ile gerçek zamanlı bildirim gönder
            if (_hubContext != null)
            {
                try
                {
                    var notificationDto = new NotificationDto
                    {
                        Id = notification.Id,
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        IsRead = false,
                        TicketId = ticketId,
                        CreatedDate = notification.CreatedDate
                    };

                    // Tüm admin kullanıcılara broadcast
                    await _hubContext.Clients.All
                        .SendAsync("AdminNotificationReceived", notificationDto)
                        .ConfigureAwait(false);

                    _logger.LogInformation("Admin'lere SignalR bildirim gönderildi: {Title}", title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Admin'lere SignalR bildirim gönderilemedi: {Title}", title);
                }
            }
        }

        
        public async Task NotifyNewTicketAsync(Ticket ticket, string tenantName)
        {
            await CreateNotificationAsync(
                "Yeni Destek Talebi",
                $"{tenantName} firmasından yeni bir destek talebi: {ticket.Title}",
                NotificationType.NewTicket,
                ticket.Id
            );

            // Admin'e email gönder
            try
            {
                await _emailService.SendNewTicketEmailToAdminAsync(
                    ticket.Title,
                    tenantName,
                    ticket.Priority.ToString(),
                    ticket.Description,
                    ticket.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin'e yeni ticket e-postası gönderilemedi: {TicketId}", ticket.Id);
            }
        }

        
        public async Task NotifyNewCommentAsync(int ticketId, string ticketTitle, string authorName, bool isCustomerComment)
        {
            if (isCustomerComment)
            {
                await CreateNotificationAsync(
                    "Yeni Müşteri Yorumu",
                    $"{authorName} bir yorum ekledi: {ticketTitle}",
                    NotificationType.TicketComment,
                    ticketId
                );
            }
        }

        
        public async Task NotifyStatusChangedAsync(int ticketId, string ticketTitle, string newStatus)
        {
            await CreateNotificationAsync(
                "Durum Değişikliği",
                $"Ticket durumu değişti: {ticketTitle} → {newStatus}",
                NotificationType.TicketStatusChanged,
                ticketId
            );
        }

        
        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.UpdatedDate = DateTimeHelper.GetLocalNow();
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Belirli bir staff'a bildirim oluşturur
        /// </summary>
        public async Task CreateStaffNotificationAsync(int staffId, string title, string message, NotificationType type, int? ticketId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                TicketId = ticketId,
                IsForAdmin = false,
                TargetUserId = staffId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Staff'a bildirim gönderildi: {StaffId}, {Title}", staffId, title);

            // SignalR aracılığıyla real-time bildirim gönder
            if (_hubContext != null)
            {
                try
                {
                    var notificationDto = new NotificationDto
                    {
                        Id = notification.Id,
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        IsRead = false,
                        TicketId = ticketId,
                        CreatedDate = notification.CreatedDate
                    };

                    await _hubContext.Clients.User(staffId.ToString())
                        .SendAsync("StaffNotificationReceived", notificationDto)
                        .ConfigureAwait(false);

                    _logger.LogInformation("Staff'a SignalR bildirim gönderildi: {StaffId}", staffId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Staff'a SignalR bildirim gönderilemedi: {StaffId}", staffId);
                }
            }
        }

        /// <summary>
        /// Ticket'a staff atandığında staff'a bildirim gönder
        /// </summary>
        public async Task NotifyTicketAssignedToStaffAsync(int staffId, string ticketTitle, string tenantName, int ticketId)
        {
            await CreateStaffNotificationAsync(
                staffId,
                "Yeni Talik Atandı",
                $"Size yeni bir ticket atandı: {ticketTitle} ({tenantName})",
                NotificationType.TicketAssigned,
                ticketId
            );

            // Staff bilgilerini al ve e-posta gönder
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff != null && !string.IsNullOrEmpty(staff.Email))
                {
                    await _emailService.SendTicketAssignmentEmailAsync(
                        staff.Email,
                        staff.FullName,
                        ticketTitle,
                        tenantName,
                        ticketId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Staff'a ticket atama e-postası gönderilemedi: {StaffId}", staffId);
            }
        }

        /// <summary>
        /// Yetkili olduğu ticket'lara yorum gelince staff'a bildirim gönder
        /// </summary>
        public async Task NotifyStaffAboutCommentAsync(int staffId, int ticketId, string ticketTitle, string authorName, string commentPreview)
        {
            await CreateStaffNotificationAsync(
                staffId,
                "Yetkili Olduğu Ticket'a Yorum",
                $"{authorName} tarafından {ticketTitle} için yorum yapıldı",
                NotificationType.TicketComment,
                ticketId
            );

            // Staff bilgilerini al ve e-posta gönder
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff != null && !string.IsNullOrEmpty(staff.Email))
                {
                    await _emailService.SendNewCommentEmailAsync(
                        staff.Email,
                        staff.FullName,
                        ticketTitle,
                        authorName,
                        commentPreview,
                        ticketId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Staff'a yeni yorum e-postası gönderilemedi: {StaffId}", staffId);
            }
        }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public int? TicketId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
