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
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            // Seed Staff Data
            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1,
                    FullName = "Ahmet Yılmaz",
                    Email = "ahmet.yilmaz@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(532) 111 2233",
                    Department = "Teknik Destek",
                    IsActive = true,
                    MaxConcurrentTickets = 10,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Staff
                {
                    Id = 2,
                    FullName = "Mehmet Kaya",
                    Email = "mehmet.kaya@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(533) 222 3344",
                    Department = "Teknik Destek",
                    IsActive = true,
                    MaxConcurrentTickets = 8,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Staff
                {
                    Id = 3,
                    FullName = "Ayşe Demir",
                    Email = "ayse.demir@kobi.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Phone = "(534) 333 4455",
                    Department = "Satış",
                    IsActive = true,
                    MaxConcurrentTickets = 5,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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