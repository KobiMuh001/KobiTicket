using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class TicketHistory : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Ticket? Ticket { get; set; } // Navigation Property

        public required string ActionBy { get; set; } // İşlemi yapan kişi
        public required string Description { get; set; } // Yapılan işlemin açıklaması
    }
}