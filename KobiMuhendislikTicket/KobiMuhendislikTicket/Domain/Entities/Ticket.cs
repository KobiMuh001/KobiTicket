using KobiMuhendislikTicket.Domain.Common;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public required string Title { get; set; } 
        public required string Description { get; set; } 

        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Low;
        public string? AssignedPerson { get; set; } 
        // Foreign Keys
        public Guid TenantId { get; set; } 
        public Tenant? Tenant { get; set; }

        public Guid? AssetId { get; set; } 
        public Asset? Asset { get; set; }
        public string? ImagePath { get; set; }
    }
}
