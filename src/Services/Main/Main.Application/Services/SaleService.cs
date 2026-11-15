using Main.Application.Dtos.Sale;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Interfaces.Services;
using Main.Application.Models;
using Main.Application.Models.Storage;
using Main.Entities.Sale;
using Main.Enums;
using MediatR;
using Main.Application.Extensions.Entities;

namespace Main.Application.Services;

public class SaleService(ISender sender, IStorageContentService storageContentService) : ISaleService
{
	public List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<NewSaleContentDto> saleContents)
		=> DistributeDetails(
			storageContentValues,
			saleContents.Select((x, index) => new SaleContentInput(
				index,
				x.ProductId,
				x.OriginalPrice,
				x.DiscountedPrice,
				x.Count,
				x.Comment)));

	public List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<EditSaleContentDto> saleContents)
		=> DistributeDetails(
			storageContentValues,
			saleContents.Select((x, index) => new SaleContentInput(
				index,
				x.ProductId,
				x.Price,
				x.PriceWithDiscount,
				x.Count,
				x.Comment)));

	public async Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
		string storageCode,
		IEnumerable<NewSaleContentDto> saleContents,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		var contents = saleContents.ToList();
		var takenFromStorage = await TakeFromStorage(
			storageCode,
			contents.Select(x => (x.ProductId, x.Count)),
			movementType,
			cancellationToken);

		return DistributeDetails(takenFromStorage, contents);
	}

	public async Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
		string storageCode,
		IEnumerable<EditSaleContentDto> saleContents,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		var contents = saleContents.ToList();
		var takenFromStorage = await TakeFromStorage(
			storageCode,
			contents.Select(x => (x.ProductId, x.Count)),
			movementType,
			cancellationToken);

		return DistributeDetails(takenFromStorage, contents);
	}

	public async Task RestoreContents(
		Sale sale,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		var toRestore = sale
			.Contents
			.SelectMany(content =>
				content.Details.Select(detail => detail.ToRestoreContentItem(content.ProductId)))
			.ToList();

		if (toRestore.Count == 0)
			return;

		await sender.Send(new RestoreContentCommand(toRestore, movementType), cancellationToken);
	}

	private static List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<SaleContentInput> saleContents)
		=> SaleContentAllocator.Distribute(
			storageContentValues.Select(lot => new AllocationLot(
				lot.Id,
				lot.ProductId,
				lot.CurrencyId,
				lot.BuyPrice,
				lot.Count,
				lot.PurchaseDatetime)),
			saleContents.Select(content => new AllocationLine(
				content.OriginalIndex,
				content.ProductId,
				content.Price,
				content.PriceWithDiscount,
				content.Count,
				content.Comment)));

	private async Task<IReadOnlyList<StorageLot>> TakeFromStorage(
		string storageCode,
		IEnumerable<(int ProductId, int Count)> contents,
		StorageMovementType movementType,
		CancellationToken cancellationToken)
		=> await storageContentService.SubtractByProductsAsync(
			contents.Select(x => new SubtractProductFromStorageItem(
					x.ProductId,
					storageCode,
					x.Count))
				.ToArray(),
			movementType,
			cancellationToken);

	private sealed record SaleContentInput(
		int OriginalIndex,
		int ProductId,
		decimal Price,
		decimal PriceWithDiscount,
		int Count,
		string? Comment);
}
