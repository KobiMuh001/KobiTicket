using Microsoft.EntityFrameworkCore;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Common;
using KobiMuhendislikTicket.Application.Common;


namespace KobiMuhendislikTicket.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Tablolar
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketImage> TicketImages { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTenant> ProductTenants { get; set; }
        // SystemParameters: generic table for lookups and operational settings
        public DbSet<KobiMuhendislikTicket.Domain.Entities.System.SystemParameter> SystemParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tenant (Firma) Ayarları
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.TaxNumber)
                .IsUnique();

            modelBuilder.Entity<Tenant>()
                .Property(t => t.TaxNumber)
                .IsRequired()
                .HasMaxLength(10);

            // Staff (Çalışan) Ayarları
            modelBuilder.Entity<Staff>()
                .ToTable("Staffs")
                .HasIndex(s => s.Email)
                .IsUnique();

            // İlişkiler
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Assets)
                .WithOne(a => a.Tenant)
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Tickets)
                .WithOne(ticket => ticket.Tenant)
                .HasForeignKey(ticket => ticket.TenantId);

            modelBuilder.Entity<Ticket>()
                .HasMany(t => t.TicketImages)
                .WithOne(i => i.Ticket)
                .HasForeignKey(i => i.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ProductTenant>()
                .HasKey(pt => new { pt.ProductId, pt.TenantId });

            modelBuilder.Entity<ProductTenant>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTenants)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductTenant>()
                .HasOne(pt => pt.Tenant)
                .WithMany(t => t.ProductTenants)
                .HasForeignKey(pt => pt.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Centralized lookup seed (single Lookups table)
            var seedDate = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc);

            // SystemParameters seeded below (central source for lookups/settings)

            // Seed SystemParameters (centralized generic table)
            modelBuilder.Entity<KobiMuhendislikTicket.Domain.Entities.System.SystemParameter>()
                .HasIndex(p => p.Group);
            modelBuilder.Entity<KobiMuhendislikTicket.Domain.Entities.System.SystemParameter>().HasData(
                // TicketPriority group
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 100, Group = "TicketPriority", Key = "Düşük", NumericKey = 1, Value = "Low", Value2 = "#6C757D", Description = "Düşük öncelik", IsActive = true, DataType = "String", SortOrder = 1 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 101, Group = "TicketPriority", Key = "Orta", NumericKey = 2, Value = "Medium", Value2 = "#17A2B8", Description = "Orta öncelik", IsActive = true, DataType = "String", SortOrder = 2 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 102, Group = "TicketPriority", Key = "Yüksek", NumericKey = 3, Value = "High", Value2 = "#FD7E14", Description = "Yüksek öncelik", IsActive = true, DataType = "String", SortOrder = 3 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 103, Group = "TicketPriority", Key = "Kritik", NumericKey = 4, Value = "Critical", Value2 = "#DC3545", Description = "Kritik öncelik", IsActive = true, DataType = "String", SortOrder = 4 },

                // TicketStatus group
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 110, Group = "TicketStatus", Key = "Açık", NumericKey = 1, Value = "Open", Value2 = "#007BFF", Description = "Açık", IsActive = true, DataType = "String", SortOrder = 1 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 111, Group = "TicketStatus", Key = "İşleniyor", NumericKey = 2, Value = "Processing", Value2 = "#FFC107", Description = "İşleniyor", IsActive = true, DataType = "String", SortOrder = 2 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 112, Group = "TicketStatus", Key = "Müşteri Bekleniyor", NumericKey = 3, Value = "WaitingForCustomer", Value2 = "#6F42C1", Description = "Müşteri Bekleniyor", IsActive = true, DataType = "String", SortOrder = 3 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 113, Group = "TicketStatus", Key = "Çözüldü", NumericKey = 4, Value = "Resolved", Value2 = "#28A745", Description = "Çözüldü", IsActive = true, DataType = "String", SortOrder = 4 }, 
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 114, Group = "TicketStatus", Key = "Kapandı", NumericKey = 5, Value = "Closed", Value2 = "#343A40", Description = "Kapandı", IsActive = true, DataType = "String", SortOrder = 5 }, 
                // UserRole group
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 120, Group = "UserRole", Key = "Admin", NumericKey = 1, Value = "Admin", Description = "Admin role", IsActive = true, DataType = "String", SortOrder = 1 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 121, Group = "UserRole", Key = "Staff", NumericKey = 2, Value = "Staff", Description = "Staff role", IsActive = true, DataType = "String", SortOrder = 2 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 122, Group = "UserRole", Key = "Customer", NumericKey = 3, Value = "Customer", Description = "Customer role", IsActive = true, DataType = "String", SortOrder = 3 },

                // Department group
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 130, Group = "Department", Key = "TeknikDestek", NumericKey = 1, Value = "Teknik Destek", Description = "Teknik Destek", IsActive = true, DataType = "String", SortOrder = 1 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 131, Group = "Department", Key = "Satis", NumericKey = 2, Value = "Satış", Description = "Satış", IsActive = true, DataType = "String", SortOrder = 2 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 132, Group = "Department", Key = "Muhasebe", NumericKey = 3, Value = "Muhasebe", Description = "Muhasebe", IsActive = true, DataType = "String", SortOrder = 3 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 133, Group = "Department", Key = "Yonetim", NumericKey = 4, Value = "Yönetim", Description = "Yönetim", IsActive = true, DataType = "String", SortOrder = 4 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 134, Group = "Department", Key = "Diger", NumericKey = 5, Value = "Diğer", Description = "Diğer", IsActive = true, DataType = "String", SortOrder = 5 },

                // Example system setting
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 200, Group = "General", Key = "DefaultTicketLimit",NumericKey = 1, Value = "15", Description = "Varsayılan ticket limiti", IsActive = true, DataType = "Int", SortOrder = 1 }
            );

            // Seed Staff Data (use DepartmentId)
            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1,
                    FullName = "Ahmet Yılmaz",
                    Email = "ahmet.yilmaz@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(532) 111 2233",
                    DepartmentId = 1,
                    IsActive = true,
                    MaxConcurrentTickets = 10,
                    CreatedDate = seedDate
                },
                new Staff
                {
                    Id = 2,
                    FullName = "Mehmet Kaya",
                    Email = "mehmet.kaya@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(533) 222 3344",
                    DepartmentId = 1,
                    IsActive = true,
                    MaxConcurrentTickets = 8,
                    CreatedDate = seedDate
                },
                new Staff
                {
                    Id = 3,
                    FullName = "Ayşe Demir",
                    Email = "ayse.demir@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(534) 333 4455",
                    DepartmentId = 2,
                    IsActive = true,
                    MaxConcurrentTickets = 5,
                    CreatedDate = seedDate
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "izRP Finans Modülü",
                    Description = "Finans süreçleri için temel ürün modülü.",
                    CreatedDate = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Product
                {
                    Id = 2,
                    Name = "izRP İnsan Kaynakları",
                    Description = "İK operasyonları için temel ürün modülü.",
                    CreatedDate = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Product
                {
                    Id = 3,
                    Name = "izRP Muhasebe",
                    Description = "Muhasebe süreçleri için temel ürün modülü.",
                    CreatedDate = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                }
            );

            base.OnModelCreating(modelBuilder);
        }

        // Otomatik Tarih Atama
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTimeHelper.GetLocalNow();
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTimeHelper.GetLocalNow();
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}