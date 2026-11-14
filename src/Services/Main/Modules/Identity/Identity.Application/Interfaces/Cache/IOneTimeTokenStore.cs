using Main.Enums;

namespace Main.Application.Interfaces.Cache;

public interface IOneTimeTokenStore
{
	Task StoreAsync(
		OneTimeTokenPurpose purpose,
		Guid tokenId,
		TimeSpan ttl);

	Task<bool> ConsumeAsync(OneTimeTokenPurpose purpose, Guid tokenId);
}
