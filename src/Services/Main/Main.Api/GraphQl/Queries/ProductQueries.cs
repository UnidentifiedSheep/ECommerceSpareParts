using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Product;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public Task<GqlProduct?> GetProductByIdAsync(
        IProductByIdDataLoader loader,
        int id,
        CancellationToken ct)
    {
        return loader.LoadAsync(id, ct);
    }
}
