using Armtek.Integrations.Core.Interfaces;
using Armtek.Integrations.Core.Responses;

namespace Armtek.Integrations.Client;

public class UserSettingsNode(
    IArmtekConnectionProvider armtekConnectionProvider
    ) : NodeBase(armtekConnectionProvider), IUserSettingsNode
{
    public async Task<GetBrandsResponse> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        var request = await GetRequest(
            HttpMethod.Get,
            "api/ws_user/getBrandList?format=json",
            cancellationToken);
        
        throw new NotImplementedException();
    }
}