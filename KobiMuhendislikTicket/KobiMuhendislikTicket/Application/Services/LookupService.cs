using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KobiMuhendislikTicket.Application.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        public LookupService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // Legacy lookup methods removed. Use GetByGroupAsync (SystemParameter) instead.

        public Task<List<KobiMuhendislikTicket.Domain.Entities.System.SystemParameter>> GetByGroupAsync(string group)
        {
            var cacheKey = $"systemparams:group:{group}";
            return _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await _db.SystemParameters
                    .Where(p => p.Group == group && p.IsActive)
                    .ToListAsync();
            });
        }
    }
}
