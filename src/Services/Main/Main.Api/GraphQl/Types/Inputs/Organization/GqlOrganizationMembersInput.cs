using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("OrganizationMembersInput")]
public sealed record GqlOrganizationMembersInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
