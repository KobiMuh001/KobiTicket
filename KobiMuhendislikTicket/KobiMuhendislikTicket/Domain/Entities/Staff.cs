using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class Staff : BaseEntity
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? Phone { get; set; }
        public string Department { get; set; } = "Teknik Destek";
        public bool IsActive { get; set; } = true;
        public int MaxConcurrentTickets { get; set; } = 10; // Aynı anda max kaç ticket alabilir
    }
}
