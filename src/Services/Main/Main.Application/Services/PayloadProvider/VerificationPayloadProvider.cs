using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Models.Auth;
using Main.Enums;
using Main.Enums.Auth;

namespace Main.Application.Services.PayloadProvider;

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
			OneTimeTokenPurpose.Verification,
			payload.Id,
			TimeSpan.FromMinutes(30));
		return payload;
	}

	public Task<bool> TryConsumeToken(Guid tokenId) => oneTimeTokenStore.ConsumeAsync(
		OneTimeTokenPurpose.Verification,
		tokenId);
}
