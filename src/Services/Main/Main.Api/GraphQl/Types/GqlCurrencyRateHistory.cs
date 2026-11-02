using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Currencies;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types;

[GraphQLName("CurrencyRateHistory")]
public sealed record GqlCurrencyRateHistory(
	[property: GraphQLIgnore] CurrencyRateHistoryDto History)
{
	[GraphQLName("id")]
	public int Id => History.Id;

	[GraphQLName("fromCurrency")]
	public async Task<GqlCurrency> GetFromCurrencyAsync(
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new(await loader.LoadAsync(History.FromCurrencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(History.FromCurrencyId));

	[GraphQLName("toCurrency")]
	public async Task<GqlCurrency> GetToCurrencyAsync(
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new(await loader.LoadAsync(History.ToCurrencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(History.ToCurrencyId));

	[GraphQLName("rate")]
	public decimal Rate => History.Rate;

	[GraphQLName("createdAt")]
	public DateTime CreatedAt => History.CreatedAt;
}
