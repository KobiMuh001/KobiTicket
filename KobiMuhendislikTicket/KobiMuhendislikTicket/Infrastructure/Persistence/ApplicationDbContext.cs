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
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 100, Group = "TicketPriority", Key = "Düşük", NumericKey = 1, Value = "Low", Description = "Düşük öncelik", IsActive = true, DataType = "String", SortOrder = 1 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 101, Group = "TicketPriority", Key = "Orta", NumericKey = 2, Value = "Medium", Description = "Orta öncelik", IsActive = true, DataType = "String", SortOrder = 2 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 102, Group = "TicketPriority", Key = "Yüksek", NumericKey = 3, Value = "High", Description = "Yüksek öncelik", IsActive = true, DataType = "String", SortOrder = 3 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 103, Group = "TicketPriority", Key = "Kritik", NumericKey = 4, Value = "Critical", Description = "Kritik öncelik", IsActive = true, DataType = "String", SortOrder = 4 },

                // TicketStatus group
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 110, Group = "TicketStatus", Key = "Açık", NumericKey = 1, Value = "Open", Description = "Açık", IsActive = true, DataType = "String", SortOrder = 1 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 111, Group = "TicketStatus", Key = "İşleniyor", NumericKey = 2, Value = "Processing", Description = "İşleniyor", IsActive = true, DataType = "String", SortOrder = 2 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 112, Group = "TicketStatus", Key = "Müşteri Bekleniyor", NumericKey = 3, Value = "WaitingForCustomer", Description = "Müşteri Bekleniyor", IsActive = true, DataType = "String", SortOrder = 3 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 113, Group = "TicketStatus", Key = "Çözüldü", NumericKey = 4, Value = "Resolved", Description = "Çözüldü", IsActive = true, DataType = "String", SortOrder = 4 },
                new KobiMuhendislikTicket.Domain.Entities.System.SystemParameter { Id = 114, Group = "TicketStatus", Key = "Kapandı", NumericKey = 5, Value = "Closed", Description = "Kapandı", IsActive = true, DataType = "String", SortOrder = 5 },

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