using GreenDonut;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class SaleDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, SaleDto>> GetSaleByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetSalesByIdsQuery(keys), cancellationToken)).Sales;
}
