using Enums;
using HotChocolate;

namespace Main.Api.GraphQl.Logistics.Types.Inputs;

[GraphQLName("DeliveryScheduleExceptionInput")]
public sealed record GqlDeliveryScheduleExceptionInput
{
	[GraphQLName("supplier")]
	public required Supplier Supplier { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("date")]
	public required DateOnly Date { get; init; }
}
