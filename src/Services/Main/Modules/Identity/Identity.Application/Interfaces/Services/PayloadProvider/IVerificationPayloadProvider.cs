using Main.Entities.Enums.Auth;
using Main.Modules.Identity.Application.Models.Auth;

namespace Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;

public interface IVerificationPayloadProvider
{
	Task<VerificationPayload> GetPayload(
		Guid userId,
		VerificationType type,
		string dataToVerify);

	Task<bool> TryConsumeToken(Guid tokenId);
}
