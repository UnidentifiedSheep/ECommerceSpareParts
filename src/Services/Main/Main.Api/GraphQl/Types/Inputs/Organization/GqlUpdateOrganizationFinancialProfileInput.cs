using HotChocolate;
using GraphQL.Common.Types;
using Main.Application.Dtos.Balances;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("UpdateOrganizationFinancialProfileInput")]
public sealed record GqlUpdateOrganizationFinancialProfileInput
{
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("minimalAllowedBalance")]
	public GqlPatchField<decimal>? MinimalAllowedBalance { get; init; }

	public PatchOrganizationFinancialProfileDto ToDto() => new()
	{
		MinimalAllowedBalance = MinimalAllowedBalance
	};
}
