using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Queries;

public sealed class ProducerQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public Task<GqlProducer?> GetProductByIdAsync(
        ProducerByIdDataLoader loader,
        int id,
        CancellationToken ct)
        => loader.LoadAsync(id, ct);
}