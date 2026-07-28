namespace Main.Application.Interfaces.Cache;

public interface IResetCache
{
    Task StoreAsync(Guid tokenId, TimeSpan ttl);
    Task<Guid?> ConsumeAsync(Guid tokenId);
}