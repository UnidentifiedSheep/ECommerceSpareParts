using Armtek.Integrations.Core.Responses;

namespace Armtek.Integrations.Core.Interfaces;

public interface IUserSettingsNode
{
    Task<GetBrandsResponse> GetBrandsAsync(CancellationToken cancellationToken = default);
}