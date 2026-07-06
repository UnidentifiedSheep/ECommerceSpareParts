using Application.Common.Interfaces.Cache;
using StackExchange.Redis;

namespace Cache.DistributedLock;

public class DistributedLockProvider(
    IDatabase database) : IDistributedLockProvider
{
    public async Task<IDistributedLockLease?> TryAcquire(string key,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");

        var acquired = await database.StringSetAsync(
            key: key,
            value: token,
            expiry: ttl,
            when: When.NotExists);

        return !acquired 
            ? null 
            : new RedisDistributedLockLease(database, key, token);
    }
    public async Task<TResult?> TryExecuteWithLock<TResult>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken ct = default)
    {
        await using var lease = await TryAcquire(key, ttl, ct);

        if (lease is null) return default;

        return await action(ct);
    }
}