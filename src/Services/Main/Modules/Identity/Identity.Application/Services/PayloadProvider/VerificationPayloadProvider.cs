using Application.Common.Interfaces.Cache;
using Main.Entities.Enums.Auth;
using Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;
using Main.Modules.Identity.Application.Models.Auth;

namespace Main.Modules.Identity.Application.Services.PayloadProvider;

public class VerificationPayloadProvider(IOneTimeTokenStore oneTimeTokenStore) : IVerificationPayloadProvider
{
	public async Task<VerificationPayload> GetPayload(
		Guid userId,
		VerificationType type,
		string dataToVerify)
	{
		var payload = new VerificationPayload
		{
			UserId = userId,
			Type = type,
			DataToVerify = dataToVerify
		};

		await oneTimeTokenStore.StoreAsync(
			OneTimeTokenPurposes.Verification,
			payload.Id,
			TimeSpan.FromMinutes(30));
		return payload;
	}

	public Task<bool> TryConsumeToken(Guid tokenId)
		=> oneTimeTokenStore.ConsumeAsync(
			OneTimeTokenPurposes.Verification,
			tokenId);
}
