using GreenDonut;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.ProducerAliases;
using Main.Application.Handlers.Producers;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class ProducerDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<int, GqlProducer>>
        GetProducerByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
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

    [DataLoader]
    public static async Task<Dictionary<int, List<string>>>
        GetProducerAliasesByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProducersAliasesQuery(keys),
                cancellationToken))
            .ProducersAliases;
    }
}
