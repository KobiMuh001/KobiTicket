using KobiMuhendislikTicket.Domain.Common;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;

namespace KobiMuhendislikTicket.Domain.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class Tenant : BaseEntity
    {
        public required string CompanyName { get; set; } 
        public required string TaxNumber { get; set; }   
        public string? TaxOffice { get; set; }
        public required string Email { get; set; }       
        public string? Username { get; set; }
        public required string PasswordHash { get; set; } 
        public required string PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }

        // Navigation Properties (İlişkiler)
        public ICollection<Asset> Assets { get; set; } = new List<Asset>(); 
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>(); 
        public ICollection<ProductTenant> ProductTenants { get; set; } = new List<ProductTenant>();
    }
}
