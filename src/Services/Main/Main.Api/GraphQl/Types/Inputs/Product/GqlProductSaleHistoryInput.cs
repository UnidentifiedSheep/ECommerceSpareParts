using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductSaleHistoryInput")]
public sealed record GqlProductSaleHistoryInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("storageCode")]
	public string? StorageCode { get; init; }

	[GraphQLName("organizationId")]
	public Guid? OrganizationId { get; init; }

	[GraphQLName("preferredOrganizationId")]
	public Guid? PreferredOrganizationId { get; init; }

	[GraphQLName("currencyId")]
	public int? CurrencyId { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("showCrosses")]
	public bool? ShowCrosses { get; init; }
}
