using GreenDonut;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Handlers.Currencies;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class CurrencyDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<int, CurrencyDto>> GetCurrencyByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetAllCurrenciesQuery(), cancellationToken))
			.Currencies
			.Where(x => keys.Contains(x.Id))
			.ToDictionary(x => x.Id);
}
