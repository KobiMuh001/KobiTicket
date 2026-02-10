using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KobiMuhendislikTicket.Application.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<List<NotificationDto>> GetStaffNotificationsAsync(Guid staffId, int take = 20)
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

        
        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.IsForAdmin && !n.IsRead && !n.IsDeleted);
        }

        
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        
        public async Task MarkAllAsReadAsync()
        {
            await _context.Notifications
                .Where(n => n.IsForAdmin && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedDate, DateTime.UtcNow));
        }

        
        public async Task CreateNotificationAsync(string title, string message, NotificationType type, Guid? ticketId = null)
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
        }

        
        public async Task NotifyNewTicketAsync(Ticket ticket, string tenantName)
        {
            await CreateNotificationAsync(
                "Yeni Destek Talebi",
                $"{tenantName} firmasından yeni bir destek talebi: {ticket.Title}",
                NotificationType.NewTicket,
                ticket.Id
            );
        }

        
        public async Task NotifyNewCommentAsync(Guid ticketId, string ticketTitle, string authorName, bool isCustomerComment)
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

        
        public async Task NotifyStatusChangedAsync(Guid ticketId, string ticketTitle, string newStatus)
        {
            await CreateNotificationAsync(
                "Durum Değişikliği",
                $"Ticket durumu değişti: {ticketTitle} → {newStatus}",
                NotificationType.TicketStatusChanged,
                ticketId
            );
        }

        
        public async Task DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Guid? TicketId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
