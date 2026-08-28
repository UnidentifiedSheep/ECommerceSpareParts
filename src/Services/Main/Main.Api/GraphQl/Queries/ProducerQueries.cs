using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.DataLoaders.Producer;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Queries;

public sealed class ProducerQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public Task<GqlProducer?> GetProducerByIdAsync(
        ProducerByIdDataLoader loader,
        int id,
        CancellationToken ct)
        => loader.LoadAsync(id, ct);
}