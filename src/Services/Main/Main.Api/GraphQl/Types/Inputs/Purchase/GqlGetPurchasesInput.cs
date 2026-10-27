using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("GetPurchasesInput")]
public sealed record GqlGetPurchasesInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("rangeStartDate")]
	public DateTime? RangeStartDate { get; init; }

	[GraphQLName("rangeEndDate")]
	public DateTime? RangeEndDate { get; init; }

	[GraphQLName("supplierOrganizationIds")]
	public IReadOnlyCollection<Guid>? SupplierOrganizationIds { get; init; }

	[GraphQLName("currencyIds")]
	public IReadOnlyCollection<int>? CurrencyIds { get; init; }

	[GraphQLName("productIds")]
	public IReadOnlyCollection<int>? ProductIds { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }
}
