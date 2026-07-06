using Application.Common.Interfaces.Cache;
using StackExchange.Redis;

namespace Cache;

public sealed class RedisDistributedLockLease(
    IDatabase db,
    string key,
    string token
) : IDistributedLockLease
{
    private const string ReleaseScript =
        """
        if redis.call("GET", KEYS[1]) == ARGV[1] then
            return redis.call("DEL", KEYS[1])
        else
            return 0
        end
        """;

    private bool _disposed;

    public string Key => key;
    public string Token => token;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await db.ScriptEvaluateAsync(
            ReleaseScript,
            [Key],
            [Token]);
    }
}