namespace Application.Common.Interfaces.Cache;

public interface IOneTimeTokenStore
{
	Task StoreAsync(
		string purpose,
		Guid tokenId,
		TimeSpan ttl);

	Task<bool> ConsumeAsync(string purpose, Guid tokenId);
}
