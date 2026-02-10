using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public required string Title { get; set; }
        public required string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        
        // İlgili ticket varsa
        public Guid? TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        
        // Bildirimin hedef kitlesi (Admin için null)
        public Guid? TargetUserId { get; set; }
        public bool IsForAdmin { get; set; } = true;
    }

    public enum NotificationType
    {
        NewTicket = 1,
        TicketComment = 2,
        TicketStatusChanged = 3,
        TicketAssigned = 4,
        TicketPriorityChanged = 5,
        TicketResolved = 6,
        General = 7
    }
}
