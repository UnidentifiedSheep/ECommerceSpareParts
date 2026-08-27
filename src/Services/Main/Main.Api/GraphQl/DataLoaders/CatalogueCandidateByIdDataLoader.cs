using GreenDonut;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.ProductEnrichment;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public class CatalogueCandidateByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<Guid, GqlCatalogueCandidate>(batchScheduler, options)
{

    protected override async Task<IReadOnlyDictionary<Guid, GqlCatalogueCandidate>>
        LoadBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
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