using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities
{
    public class Asset : BaseEntity
    {
        public required string ProductName { get; set; } // Örn: izRP Finans Modülü
        public required string SerialNumber { get; set; } // Varsa donanım seri no
        public DateTime WarrantyEndDate { get; set; } // Garanti bitişi

        // Foreign Key (Hangi firmaya ait?)
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public string Status { get; set; } = "Aktif";
    }
}
