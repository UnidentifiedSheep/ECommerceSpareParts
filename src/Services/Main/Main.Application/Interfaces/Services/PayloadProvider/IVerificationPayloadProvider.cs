using Main.Application.Models.Auth;
using Main.Enums.Auth;

namespace Main.Application.Interfaces.Services.PayloadProvider;

public interface IVerificationPayloadProvider
{
	Task<VerificationPayload> GetPayload(
		Guid userId,
		VerificationType type,
		string dataToVerify);

	Task<bool> TryConsumeToken(Guid tokenId);
}
