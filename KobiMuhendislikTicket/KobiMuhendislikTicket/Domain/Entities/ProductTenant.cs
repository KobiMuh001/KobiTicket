namespace KobiMuhendislikTicket.Domain.Entities
{
    public class ProductTenant
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public DateTime? WarrantyEndDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }
}
