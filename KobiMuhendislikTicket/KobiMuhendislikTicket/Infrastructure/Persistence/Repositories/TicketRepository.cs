using Microsoft.EntityFrameworkCore;
using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketRepository(ApplicationDbContext context) => _context = context;

        // 1. Durum Güncelleme: ExecuteUpdateAsync EN GARANTİ YOLDUR. 
        // Eğer bu değişmiyorsa, ID yanlış gönderiliyor veya DB bağlantısı hatalıdır.
        public async Task UpdateStatusDirectlyAsync(int id, TicketStatus newStatus)
        {
            var affectedRows = await _context.Tickets
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Status, newStatus)
                    .SetProperty(t => t.UpdatedDate, DateTimeHelper.GetLocalNow()));

            // Kurumsal İpucu: Etkilenen satır sayısını kontrol et. 0 ise ID yanlıştır.
            if (affectedRows == 0)
            {
                throw new Exception($"Güncelleme başarısız! {id} ID'li bilet bulunamadı.");
            }
        }

        // 2. Tekil Bilet Getirme: Include'lar kurumsal raporlama için şarttır.
        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Product)
                .Include(t => t.Tenant)
                .Include(t => t.TicketImages)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // 3. Genel Listeleme: Dashboard ve Admin paneli için ana veri kaynağı.
        public async Task<List<Ticket>> GetAllAsync() =>
            await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Product)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

        // 4. Firmaya Göre Listeleme: Müşteri paneli için.
        public async Task<List<Ticket>> GetByTenantIdAsync(int tenantId) =>
            await _context.Tickets
                .Include(t => t.Product)
                .Where(t => t.TenantId == tenantId)
                .ToListAsync();

        // 5. Yeni Kayıt: Bilet ve Yorum ekleme işlemleri.
        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task AddCommentAsync(TicketComment comment)
        {
            await _context.TicketComments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        // 6. UpdateAsync: NotImplementedException'dan kurtulalım, kurumsal yedek kalsın.
        public async Task UpdateAsync(Ticket ticket)
        {
            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task AddHistoryAsync(TicketHistory history)
        {
            await _context.TicketHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }
        public async Task AssignToStaffAsync(int ticketId, string staffName)
        {
            await _context.Tickets
                .Where(t => t.Id == ticketId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.AssignedPerson, staffName)); 
        }

        public async Task UpdatePriorityDirectlyAsync(int id, TicketPriority newPriority)
        {
            
            var affectedRows = await _context.Tickets
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Priority, newPriority)
                    .SetProperty(t => t.UpdatedDate, DateTimeHelper.GetLocalNow()));

            
            if (affectedRows == 0)
            {
                throw new KeyNotFoundException($"{id} ID'li bilet bulunamadı.");
            }
        }

        public Task UpdatePriorityAsync(int ticketId, TicketPriority priority)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetFilteredTicketsAsync(int? tenantId, TicketStatus? status, TicketPriority? priority, string? assignedPerson)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetTotalTenantsCountAsync()
        {
            return await _context.Tenants.CountAsync();
        }

        public async Task<int> GetTotalAssetsCountAsync()
        {
            return await _context.Assets.CountAsync();
        }

        public async Task<List<TicketHistory>> GetHistoryAsync(int ticketId)
        {
            return await _context.TicketHistories
                .Where(h => h.TicketId == ticketId)
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<TicketComment>> GetCommentsAsync(int ticketId)
        {
            return await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<(List<Ticket> tickets, int totalCount)> GetAllTicketsPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Product)
                .OrderByDescending(t => t.CreatedDate);

            var totalCount = await query.CountAsync();
            var tickets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }
    }
}