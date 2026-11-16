using Application.Common.Interfaces.Cache;

namespace Cache.OneTimeTokens;

public class OneTimeTokenStore(ICache cache) : IOneTimeTokenStore
{
	public Task StoreAsync(string purpose, Guid tokenId, TimeSpan ttl)
		=> cache.SetAsync(Key(purpose, tokenId), tokenId, ttl);

	public async Task<bool> ConsumeAsync(string purpose, Guid tokenId)
	{
		var stored = await cache.GetDeleteAsync<Guid?>(Key(purpose, tokenId));
		return stored.HasValue && stored.Value == tokenId;
	}

	private static string Key(string purpose, Guid tokenId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
		return $"ont-time-token:{purpose}:{tokenId}";
	}
}
