using GreenDonut;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.ProductEnrichment;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class CatalogueCandidateDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, GqlCatalogueCandidate>>
        GetCatalogueCandidateByIdAsync(
            IReadOnlyList<Guid> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCatalogueCandidatesByIdsQuery(keys),
            cancellationToken);

        return result.Candidates
            .ToDictionary(
                x => x.Id,
                x => new GqlCatalogueCandidate(x));
    }
}
