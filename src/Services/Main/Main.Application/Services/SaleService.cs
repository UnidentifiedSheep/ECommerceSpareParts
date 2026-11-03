using Main.Application.Dtos.Sale;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Interfaces.Services;
using Main.Application.Models;
using Main.Application.Models.Storage;
using Main.Entities.Sale;
using Main.Enums;
using MediatR;

namespace Main.Application.Services;

public class SaleService(ISender sender, IStorageContentService storageContentService) : ISaleService
{
	public List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<NewSaleContentDto> saleContents)
	{
		return DistributeDetails(
			storageContentValues,
			saleContents.Select((x, index) => new SaleContentInput(
				index,
				x.ProductId,
				x.OriginalPrice,
				x.DiscountedPrice,
				x.Count,
				x.Comment)));
	}

	public List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<EditSaleContentDto> saleContents)
	{
		return DistributeDetails(
			storageContentValues,
			saleContents.Select((x, index) => new SaleContentInput(
				index,
				x.ProductId,
				x.Price,
				x.PriceWithDiscount,
				x.Count,
				x.Comment)));
	}

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
				content.Details.Select(detail => new RestoreContentItem(detail, content.ProductId)))
			.ToList();

		if (toRestore.Count == 0)
			return;

		await sender.Send(new RestoreContentCommand(toRestore, movementType), cancellationToken);
	}

	private List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<SaleContentInput> saleContents)
	{
		var result = new List<(int OriginalIndex, SaleContent Content)>();

		var storageByProduct = storageContentValues
			.Select(x =>
			{
				if (x.Count <= 0)
					throw new InvalidOperationException("Invalid taken quantity");

				return new
				{
					x.ProductId,
					Detail = SaleContentDetail.Create(
						x.Id,
						x.CurrencyId,
						x.BuyPrice,
						x.Count,
						x.PurchaseDatetime)
				};
			})
			.GroupBy(x => x.ProductId)
			.ToDictionary(
				g => g.Key,
				g => g
					.Select(x => x.Detail)
					.OrderByDescending(d => d.Count)
					.ThenByDescending(d => d.BuyPrice)
					.ToList());

		foreach (var contentInput in saleContents.OrderByDescending(x => x.Count))
		{
			var (originalIndex, productId, price, priceWithDiscount, count, comment) = contentInput;
			if (!storageByProduct.TryGetValue(productId, out var storage))
				throw new InvalidOperationException($"No storage for product {productId}");

			var details = new List<SaleContentDetail>();
			var leftToDistribute = count;

			var i = 0;

			while (leftToDistribute > 0 && i < storage.Count)
			{
				var detail = storage[i];

				if (leftToDistribute >= detail.Count)
				{
					details.Add(detail);
					leftToDistribute -= detail.Count;
					storage.RemoveAt(i);
				}
				else
				{
					details.Add(
						SaleContentDetail.Create(
							detail.StorageContentId,
							detail.CurrencyId,
							detail.BuyPrice,
							leftToDistribute,
							detail.PurchaseDatetime));

					storage[i] = SaleContentDetail.Create(
						detail.StorageContentId,
						detail.CurrencyId,
						detail.BuyPrice,
						detail.Count - leftToDistribute,
						detail.PurchaseDatetime);

					leftToDistribute = 0;
				}
			}

			if (leftToDistribute != 0)
				throw new InvalidOperationException("Unable to distribute details");

			var content = SaleContent.Create(
				productId,
				price,
				priceWithDiscount,
				details);

			content.SetComment(comment);

			result.Add((originalIndex, content));
		}

		return result.OrderBy(x => x.OriginalIndex).Select(x => x.Content).ToList();
	}

	private async Task<IReadOnlyList<StorageLot>> TakeFromStorage(
		string storageCode,
		IEnumerable<(int ProductId, int Count)> contents,
		StorageMovementType movementType,
		CancellationToken cancellationToken)
	{
		return await storageContentService.SubtractAsync(
				contents.Select(x => new SubtractProductFromStorageItem(
					x.ProductId,
					storageCode,
					x.Count)).ToArray(),
				movementType,
			cancellationToken);
	}

	private sealed record SaleContentInput(
		int OriginalIndex,
		int ProductId,
		decimal Price,
		decimal PriceWithDiscount,
		int Count,
		string? Comment);
}
