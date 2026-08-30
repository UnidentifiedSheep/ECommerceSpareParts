using Application.Common.Interfaces.Cache;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Enums;

namespace Main.Cache;

public class OneTimeTokenStore(ICache cache) : IOneTimeTokenStore
{
	public Task StoreAsync(
		OneTimeTokenPurpose purpose,
		Guid tokenId,
		TimeSpan ttl) => cache.SetAsync(
		CacheKeys.OneTimeTokenCache.OneTimeToken(purpose, tokenId),
		tokenId,
		ttl);

	public async Task<bool> ConsumeAsync(OneTimeTokenPurpose purpose, Guid tokenId)
	{
		var res = await cache.GetDeleteAsync<Guid?>(
			CacheKeys.OneTimeTokenCache.OneTimeToken(purpose, tokenId));
		return res.HasValue && res.Value == tokenId;
	}
}
