using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Models.Auth;
using Main.Enums;
using Main.Enums.Auth;

namespace Main.Application.Services.PayloadProvider;

public class ResetPayloadProvider(IOneTimeTokenStore oneTimeTokenStore) : IResetPayloadProvider
{
	public async Task<ResetPayload> GetPayload(Guid userId, ResetType type)
	{
		var payload = new ResetPayload
		{
			UserId = userId, Type = type
		};

		await oneTimeTokenStore.StoreAsync(
			OneTimeTokenPurpose.Reset,
			payload.Id,
			TimeSpan.FromMinutes(15));
		return payload;
	}

	public Task<bool> IsTokenValid(Guid tokenId) => oneTimeTokenStore.ConsumeAsync(
		OneTimeTokenPurpose.Reset,
		tokenId);
}
