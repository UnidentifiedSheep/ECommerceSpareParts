namespace Main.Application.Interfaces.Cache;

public interface IOneTokenStore
{
    Task StoreAsync(Guid tokenId, TimeSpan ttl);
    Task<bool> ConsumeAsync(Guid tokenId);
}