using Main.Application.Models.Auth;
using Main.Enums.Auth;

namespace Main.Application.Interfaces.Services.PayloadProvider;

public interface IResetPayloadProvider
{
    Task<ResetPayload> GetPayload(Guid userId, ResetType type);
    Task<bool> IsResetTokenValid(Guid tokenId);
}