using Enums;
using HotChocolate;

namespace Main.Api.GraphQl.Logistics.Types.Inputs;

[GraphQLName("UpdateDeliveryScheduleInput")]
public sealed record GqlUpdateDeliveryScheduleInput
{
	[GraphQLName("supplier")]
	public required Supplier Supplier { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("days")]
	public required IReadOnlyCollection<DayOfWeek> Days { get; init; }

	[GraphQLName("arrivalTime")]
	public required TimeOnly ArrivalTime { get; init; }
}
