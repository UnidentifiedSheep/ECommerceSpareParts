using Main.Entities.Enums.Auth;
using Main.Modules.Identity.Application.Models.Auth;

namespace Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;

public interface IResetPayloadProvider
{
	Task<ResetPayload> GetPayload(Guid userId, ResetType type);

	Task<bool> IsTokenValid(Guid tokenId);
}
