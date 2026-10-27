using GraphQL.Common.Types;
using HotChocolate;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.Sale;

[GraphQLName("GetSalesInput")]
public sealed record GqlGetSalesInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("rangeStartDate")]
	public DateTime? RangeStartDate { get; init; }

	[GraphQLName("rangeEndDate")]
	public DateTime? RangeEndDate { get; init; }

	[GraphQLName("organizationIds")]
	public IReadOnlyCollection<Guid>? OrganizationIds { get; init; }

	[GraphQLName("currencyIds")]
	public IReadOnlyCollection<int>? CurrencyIds { get; init; }

	[GraphQLName("productIds")]
	public IReadOnlyCollection<int>? ProductIds { get; init; }

	[GraphQLName("states")]
	public IReadOnlyCollection<SaleState>? States { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }
}
