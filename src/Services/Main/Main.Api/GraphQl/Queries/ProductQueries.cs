using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.Products;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
    [GraphQLName("byId")]
    [RequireAnyPermission(PermissionCodes.ARTICLES_GET_MAIN)]
    public async Task<GqlProduct> GetProductByIdAsync(
        ISender sender,
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), ct);
        return new GqlProduct(result.Product);
    }
}