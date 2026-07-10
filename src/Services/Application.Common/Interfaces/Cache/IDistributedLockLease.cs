namespace Application.Common.Interfaces.Cache;

public interface IDistributedLockLease : IAsyncDisposable
{
    string Key { get; }
    string Token { get; }
}