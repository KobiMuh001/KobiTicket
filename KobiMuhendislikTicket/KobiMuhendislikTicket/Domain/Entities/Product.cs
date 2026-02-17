using KobiMuhendislikTicket.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace KobiMuhendislikTicket.Domain.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<ProductTenant> ProductTenants { get; set; } = new List<ProductTenant>();
    }
}
