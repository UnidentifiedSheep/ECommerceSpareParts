using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Models.Auth;
using Main.Enums.Auth;

namespace Main.Application.Services.PayloadProvider;

public class ResetPayloadProvider(
    IOneTokenStore oneTokenStore) : IResetPayloadProvider
{
    public async Task<ResetPayload> GetPayload(Guid userId, ResetType type)
    {
        var payload = new ResetPayload
        {
            UserId = userId,
            Type = type
        };

        await oneTokenStore.StoreAsync(payload.Id, TimeSpan.FromMinutes(15));
        return payload;
    }
    
    public Task<bool> IsResetTokenValid(Guid tokenId)
        => oneTokenStore.ConsumeAsync(tokenId);
}