using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Order;

[GraphQLName("ChangeOrderStatusInput")]
public sealed record GqlChangeOrderStatusInput
{
	[GraphQLName("orderId")]
	public required Guid OrderId { get; init; }

	[GraphQLName("rowVersion")]
	public required uint RowVersion { get; init; }
}
