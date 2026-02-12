using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class TicketImage : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public required string ImagePath { get; set; }
    }
}
