using Integrations.Common;

namespace Armtek.Integrations.Core.Responses;

public record GetBrandsResponse : Response<GetBrandsResponse>
{
    public IReadOnlyList<string> Brands { get; init; } = [];
}