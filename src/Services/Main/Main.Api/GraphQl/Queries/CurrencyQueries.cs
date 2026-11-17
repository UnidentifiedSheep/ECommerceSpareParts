using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Modules.Currency.Application.Handlers.Currencies;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class CurrencyQueries
{
	[GraphQLName("byId")]
	[Lookup]
	public async Task<GqlCurrency?> GetCurrencyByIdAsync(
		ICurrencyByIdDataLoader loader,
		int id,
		CancellationToken ct)
	{
		var currency = await loader.LoadAsync(id, ct);
		return currency is null ? null : new GqlCurrency(currency);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.CURRENCIES_GET)]
	public async Task<IReadOnlyList<GqlCurrency>> ListAsync(
		ISender sender,
		CancellationToken ct)
		=> (await sender.Send(new GetAllCurrenciesQuery(), ct)).Currencies
			.Select(currency => new GqlCurrency(currency))
			.ToArray();

	[GraphQLName("base")]
	[RequireAllPermissions(PermissionCodes.CURRENCIES_GET)]
	public async Task<GqlCurrency> GetBaseAsync(ISender sender, CancellationToken ct)
		=> new((await sender.Send(new GetBaseCurrencyQuery(), ct)).Currency);
}
