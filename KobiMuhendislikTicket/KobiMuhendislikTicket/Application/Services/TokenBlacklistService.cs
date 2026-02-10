using Microsoft.Extensions.Caching.Memory;

namespace KobiMuhendislikTicket.Application.Services
{
    public interface ITokenBlacklistService
    {
        void BlacklistToken(string jti, DateTime expiresAt);
        bool IsTokenBlacklisted(string jti);
    }

    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void BlacklistToken(string jti, DateTime expiresAt)
        {
            // Token'ın expire süresine kadar blacklist'te tut
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            };
            _cache.Set($"blacklist_{jti}", true, cacheOptions);
        }

        public bool IsTokenBlacklisted(string jti)
        {
            return _cache.TryGetValue($"blacklist_{jti}", out _);
        }
    }
}
