using GreenDonut;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies;
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
