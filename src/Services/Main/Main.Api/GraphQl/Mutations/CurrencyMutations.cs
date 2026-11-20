using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Currency;
using Main.Modules.Currency.Application.Commands.Currencies.CreateCurrency;
using Main.Modules.Currency.Application.Commands.Currencies.UpdateCurrenciesRates;
using Main.Modules.Currency.Application.Queries.Currencies;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class CurrencyMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.CURRENCIES_CREATE)]
	public async Task<GqlCurrency> CreateAsync(
		GqlCreateCurrencyInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var created = await sender.Send(
			new CreateCurrencyCommand(
				input.ShortName,
				input.Name,
				input.CurrencySign,
				input.Code),
			cancellationToken);
		var result = await sender.Send(new GetCurrencyByIdQuery(created.Id), cancellationToken);

		return new GqlCurrency(result.Currency);
	}

	[GraphQLName("updateRates")]
	[RequireAllPermissions(PermissionCodes.CURRENCIES_CREATE)]
	public async Task<bool> UpdateRatesAsync(ISender sender, CancellationToken cancellationToken)
	{
		await sender.Send(new UpdateCurrenciesRatesCommand(), cancellationToken);
		return true;
	}
}
