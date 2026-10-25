using HotChocolate;
using Main.Application.Dtos.Balances;

namespace Main.Api.GraphQl.Types;

[GraphQLName("OrganizationFinancialProfile")]
public sealed record GqlOrganizationFinancialProfile(
	[property: GraphQLIgnore] OrganizationFinancialProfileDto ProfileDto)
{
	[GraphQLName("netPositionInBaseCurrency")]
	public decimal NetPositionInBaseCurrency => ProfileDto.NetPositionInBaseCurrency;

	[GraphQLName("minimalAllowedBalance")]
	public decimal MinimalAllowedBalance => ProfileDto.MinimalAllowedBalance;
}
