using GraphQL.Common.Types;
using HotChocolate;
using Main.Enums.Organization;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("GetOrganizationsInput")]
public sealed record GqlGetOrganizationsInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("userId")]
	public Guid? UserId { get; init; }

	[GraphQLName("ids")]
	public IReadOnlyCollection<Guid>? Ids { get; init; }

	[GraphQLName("types")]
	public IReadOnlyCollection<OrganizationType>? Types { get; init; }

	[GraphQLName("showHidden")]
	public bool? ShowHidden { get; init; }
}
