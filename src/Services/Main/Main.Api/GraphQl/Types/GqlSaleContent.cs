using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Sale;

namespace Main.Api.GraphQl.Types;

[GraphQLName("SaleContent")]
public sealed record GqlSaleContent([property: GraphQLIgnore] SaleContentDto Content)
{
	[GraphQLName("id")]
	public int Id => Content.Id;

	[GraphQLName("count")]
	public int Count => Content.Count;

	[GraphQLName("price")]
	public decimal Price => Content.Price;

	[GraphQLName("totalSum")]
	public decimal TotalSum => Content.TotalSum;

	[GraphQLName("comment")]
	public string? Comment => Content.Comment;

	[GraphQLName("discount")]
	public decimal Discount => Content.Discount;

	[GraphQLName("product")]
	public GqlProduct Product => new(Content.Product);

	[GraphQLName("details")]
	[RequireAllPermissions(PermissionCodes.SALES_GET)]
	public async Task<IReadOnlyList<GqlSaleContentDetail>> GetDetailsAsync(
		ISaleContentDetailsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(detail => new GqlSaleContentDetail(detail)).ToArray();
}
