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
        public string? TicketCode { get; set; } // T00001 format

        // Foreign Keys
        public int TenantId { get; set; } 
        public Tenant? Tenant { get; set; }

        public int? AssetId { get; set; } 
        public Asset? Asset { get; set; }
        public string? ImagePath { get; set; }
    }
}
