namespace KobiMuhendislikTicket.Application.DTOs
{
    public class CreateAssetDto
    {
        public required string ProductName { get; set; } 
        public required string SerialNumber { get; set; } 
        public int TenantId { get; set; } 
        public DateTime? WarrantyEndDate { get; set; } 
    }

    public class UpdateAssetDto
    {
        public required string ProductName { get; set; }
        public required string SerialNumber { get; set; }
        public required string Status { get; set; }
        public int TenantId { get; set; }
        public DateTime WarrantyEndDate { get; set; }
    }
}