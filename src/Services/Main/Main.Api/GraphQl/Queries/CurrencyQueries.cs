using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Queries;

public sealed class CurrencyQueries
{
	[GraphQLName("byId")]
	[HotChocolate.Types.Composite.LookupAttribute]
	public async Task<GqlCurrency?> GetCurrencyByIdAsync(
		ICurrencyByIdDataLoader loader,
		int id,
		CancellationToken ct)
	{
		var currency = await loader.LoadAsync(id, ct);
		return currency is null ? null : new GqlCurrency(currency);
	}
}
