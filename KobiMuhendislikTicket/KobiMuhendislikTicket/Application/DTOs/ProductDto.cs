namespace KobiMuhendislikTicket.Application.DTOs
{
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TenantCount { get; set; }
    }

    public class ProductTenantItemDto
    {
        public int TenantId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }

    public class ProductTenantsDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ProductTenantItemDto> Tenants { get; set; } = new();
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AssignProductTenantDto
    {
        public DateTime WarrantyEndDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }

    public class UpdateProductTenantDto
    {
        public DateTime WarrantyEndDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }

    public class TenantProductItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }
}
