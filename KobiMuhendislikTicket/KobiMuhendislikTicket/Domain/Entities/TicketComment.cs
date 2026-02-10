using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class TicketComment : BaseEntity
    {
        public required string Message { get; set; }
        public Guid TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // Mesajı kim yazdı? (Firma mı yoksa Kobi Mühendislik personeli mi?)
        public required string AuthorName { get; set; }
        public bool IsAdminReply { get; set; } // Admin mi cevap verdi?
    }
}