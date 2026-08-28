using GreenDonut;
using Main.Api.GraphQl.Types;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Api.GraphQl.DataLoaders;

public sealed class ProducerByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<int, GqlProducer>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<int, GqlProducer>>
        LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
            new GetProducersByIdsQuery(keys),
            cancellationToken))
            .Producers
            .ToDictionary(
                x => x.Key,
                x => new GqlProducer(x.Value));
    }
}
