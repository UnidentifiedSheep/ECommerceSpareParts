using GreenDonut;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class PurchaseDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, PurchaseDto>> GetPurchaseByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetPurchasesByIdsQuery(keys), cancellationToken)).Purchases;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, PurchaseDto>> GetPurchaseByTransactionIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetPurchasesByTransactionIdsQuery(keys), cancellationToken)).Purchases;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<PurchaseContentDto>>> GetPurchaseContentsByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken) =>
		(await sender.Send(new GetPurchasesContentsByIdsQuery(keys), cancellationToken)).Contents;

}
