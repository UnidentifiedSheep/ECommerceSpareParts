using GreenDonut;
using Main.Application.Handlers.ProducerAliases;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders.Producer;

public class ProducerAliasesByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<int, List<string>>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<int, List<string>>>
        LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProducersAliasesQuery(keys),
                cancellationToken))
            .ProducersAliases;
    }
}