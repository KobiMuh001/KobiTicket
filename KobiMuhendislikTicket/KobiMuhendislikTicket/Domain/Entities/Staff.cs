using KobiMuhendislikTicket.Domain.Common;
using KobiMuhendislikTicket.Domain.Entities.System;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class Staff : BaseEntity
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? Phone { get; set; }

        // Department is represented by SystemParameter
        public int DepartmentId { get; set; } = 1;
        public SystemParameter? Department { get; set; }

        public bool IsActive { get; set; } = true;
        public int MaxConcurrentTickets { get; set; } = 10; // Aynı anda max kaç ticket alabilir
    }
}
