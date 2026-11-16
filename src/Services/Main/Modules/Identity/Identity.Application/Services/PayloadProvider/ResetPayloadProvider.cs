using Application.Common.Interfaces.Cache;
using Main.Entities.Enums.Auth;
using Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;
using Main.Modules.Identity.Application.Models.Auth;

namespace Main.Modules.Identity.Application.Services.PayloadProvider;

public class ResetPayloadProvider(IOneTimeTokenStore oneTimeTokenStore) : IResetPayloadProvider
{
	public async Task<ResetPayload> GetPayload(Guid userId, ResetType type)
	{
		var payload = new ResetPayload
		{
			UserId = userId, Type = type
		};

		await oneTimeTokenStore.StoreAsync(
			OneTimeTokenPurposes.Reset,
			payload.Id,
			TimeSpan.FromMinutes(15));
		return payload;
	}

	public Task<bool> IsTokenValid(Guid tokenId)
		=> oneTimeTokenStore.ConsumeAsync(
			OneTimeTokenPurposes.Reset,
			tokenId);
}
