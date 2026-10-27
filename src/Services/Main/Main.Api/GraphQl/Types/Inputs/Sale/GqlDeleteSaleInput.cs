using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Sale;

[GraphQLName("DeleteSaleInput")]
public sealed record GqlDeleteSaleInput
{
	[GraphQLName("saleId")]
	public required Guid SaleId { get; init; }

	[GraphQLName("rowVersion")]
	public required uint RowVersion { get; init; }
}
