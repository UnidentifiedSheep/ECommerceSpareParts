using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.DataLoaders.Product;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.Products;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public Task<GqlProduct?> GetProductByIdAsync(
        ProductByIdDataLoader loader,
        int id,
        CancellationToken ct)
    {
        return loader.LoadAsync(id, ct);
    }
}