using Microsoft.EntityFrameworkCore;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Infrastructure.Persistence;

namespace KobiMuhendislikTicket.Infrastructure.Persistence.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant?> GetByIdAsync(int id) => await _context.Tenants.FindAsync(id);

        public async Task<Tenant?> GetByTaxNumberAsync(string taxNumber) =>
            await _context.Tenants.FirstOrDefaultAsync(t => t.TaxNumber == taxNumber);

        public async Task<List<Tenant>> GetAllAsync() => await _context.Tenants.ToListAsync();

        public async Task AddAsync(Tenant tenant)
        {
            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tenant = await GetByIdAsync(id);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
            }
        }
    }
}