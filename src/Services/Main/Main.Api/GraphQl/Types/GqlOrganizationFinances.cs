using HotChocolate;
using Main.Application.Dtos.Balances;

namespace Main.Api.GraphQl.Types;

[GraphQLName("OrganizationFinances")]
public sealed record GqlOrganizationFinances(
	[property: GraphQLIgnore] OrganizationFinancialProfileDto? Profile,
	[property: GraphQLIgnore] IReadOnlyCollection<OrganizationBalanceDto> BalanceDtos)
{
	[GraphQLName("financialProfile")]
	public GqlOrganizationFinancialProfile? FinancialProfile
		=> Profile is null ? null : new(Profile);

	[GraphQLName("balances")]
	public IReadOnlyList<GqlOrganizationBalance> Balances =>
		BalanceDtos.Select(balance => new GqlOrganizationBalance(balance)).ToArray();
}
