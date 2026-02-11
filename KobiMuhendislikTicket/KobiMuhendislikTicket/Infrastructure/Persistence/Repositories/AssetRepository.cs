using Microsoft.EntityFrameworkCore;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;

namespace KobiMuhendislikTicket.Infrastructure.Persistence.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly ApplicationDbContext _context;
        public AssetRepository(ApplicationDbContext context) => _context = context;

        
        public async Task<List<Asset>> GetAllAsync()
        {
            return await _context.Assets
                .Include(a => a.Tenant) 
                .Where(a => !a.IsDeleted) 
                .ToListAsync();
        }

        public async Task<List<Asset>> GetByTenantIdAsync(int tenantId) =>
            await _context.Assets
                .Include(a => a.Tenant)
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();

        public async Task<Asset?> GetByIdAsync(int id) =>
            await _context.Assets
                .Include(a => a.Tenant)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        public async Task<Asset?> GetBySerialNumberAsync(string serialNumber) =>
            await _context.Assets
                .FirstOrDefaultAsync(a => a.SerialNumber == serialNumber && !a.IsDeleted);

        public async Task AddAsync(Asset asset)
        {
            await _context.Assets.AddAsync(asset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Asset asset)
        {
            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                asset.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}