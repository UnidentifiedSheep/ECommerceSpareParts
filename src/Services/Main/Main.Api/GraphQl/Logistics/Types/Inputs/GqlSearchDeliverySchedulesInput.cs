using Enums;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Logistics.Types.Inputs;

[GraphQLName("SearchDeliverySchedulesInput")]
public sealed record GqlSearchDeliverySchedulesInput
{
	[GraphQLName("suppliers")]
	public IReadOnlyCollection<Supplier> Suppliers { get; init; } = [];

	[GraphQLName("storageCode")]
	public string? StorageCode { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
