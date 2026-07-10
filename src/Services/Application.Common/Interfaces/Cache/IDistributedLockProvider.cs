namespace Application.Common.Interfaces.Cache;

public interface IDistributedLockProvider
{
    Task<IDistributedLockLease?> TryAcquire(
        string key,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task<TResult?> TryExecuteWithLock<TResult>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken ct = default);
}