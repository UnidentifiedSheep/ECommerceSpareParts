using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.Products;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
    [GraphQLName("byId")]
    [Lookup]
    public async Task<GqlProduct?> GetProductByIdAsync(
        ISender sender,
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), ct);
        return new GqlProduct(result.Product);
    }
    
    [GraphQLName("byIds")]
    public async Task<List<GqlProduct>> GetProductByIdsAsync(
        ISender sender,
        IEnumerable<int> ids,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdsQuery(ids), ct);
        return result.Products.Select(x => new GqlProduct(x)).ToList();
    }
}