using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Product;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public async Task<GqlProduct?> GetProductByIdAsync(
        IProductByIdDataLoader loader,
        int id,
        CancellationToken cancellationToken)
    {
        var product = await loader.LoadAsync(id, cancellationToken);
        return product is null ? null : new GqlProduct(product);
    }
}
