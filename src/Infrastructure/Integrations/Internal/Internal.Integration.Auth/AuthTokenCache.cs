using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Internal.Integration.Auth;

public interface IAuthTokenCache
{
    Task<string> GetOrCreateAsync(
        string service,
        string serviceSecret,
        Func<CancellationToken, Task<string>> tokenFactory,
        CancellationToken cancellationToken = default);
}

public sealed class AuthTokenCache : IAuthTokenCache
{
    private static readonly JwtSecurityTokenHandler JwtHandler = new();

    private static readonly TimeSpan RefreshBeforeExpiration = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<AuthTokenCacheKey, CachedToken> _cache = new();

    private readonly ConcurrentDictionary<AuthTokenCacheKey, SemaphoreSlim> _locks = new();

    public async Task<string> GetOrCreateAsync(
        string service,
        string serviceSecret,
        Func<CancellationToken, Task<string>> tokenFactory,
        CancellationToken cancellationToken = default)
    {
        var key = AuthTokenCacheKey.Create(service, serviceSecret);

        if (_cache.TryGetValue(key, out var cachedToken) && cachedToken.IsValid)
            return cachedToken.Token;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(key, out cachedToken) && cachedToken.IsValid)
                return cachedToken.Token;

            var newToken = await tokenFactory(cancellationToken);

            var expiresAtUtc = GetExpiresAtUtc(newToken);

            var tokenToCache = new CachedToken(
                Token: newToken,
                ExpiresAtUtc: expiresAtUtc);

            _cache[key] = tokenToCache;

            return newToken;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static DateTime GetExpiresAtUtc(string token)
    {
        var jwt = JwtHandler.ReadJwtToken(token);

        return DateTime.SpecifyKind(jwt.ValidTo, DateTimeKind.Utc);
    }

    private sealed record CachedToken(
        string Token,
        DateTime ExpiresAtUtc)
    {
        public bool IsValid =>
            DateTime.UtcNow < ExpiresAtUtc.Subtract(RefreshBeforeExpiration);
    }

    private sealed record AuthTokenCacheKey(
        string Service,
        string SecretHash)
    {
        public static AuthTokenCacheKey Create(string service, string secret)
        {
            return new AuthTokenCacheKey(
                Service: service,
                SecretHash: Sha256(secret));
        }

        private static string Sha256(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}