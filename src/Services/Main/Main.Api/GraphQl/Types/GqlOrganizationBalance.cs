using HotChocolate;
using Main.Application.Dtos.Balances;

namespace Main.Api.GraphQl.Types;

[GraphQLName("OrganizationBalance")]
public sealed record GqlOrganizationBalance([property: GraphQLIgnore] OrganizationBalanceDto BalanceDto)
{
	[GraphQLName("balance")]
	public decimal Balance => BalanceDto.Balance;

	[GraphQLName("currency")]
	public GqlCurrency Currency => new(BalanceDto.Currency);
}
