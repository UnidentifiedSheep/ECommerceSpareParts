using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Entities.Currency;
using Main.Modules.Balance.Application.Dtos.Balances;

namespace Main.Api.GraphQl.Types.Organization;

[GraphQLName("OrganizationBalance")]
public sealed record GqlOrganizationBalance([property: GraphQLIgnore]
	OrganizationBalanceDto BalanceDto)
{
	[GraphQLName("balance")]
	public decimal Balance => BalanceDto.Balance;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> new(
			await loader.LoadAsync(BalanceDto.CurrencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(BalanceDto.CurrencyId));
}
