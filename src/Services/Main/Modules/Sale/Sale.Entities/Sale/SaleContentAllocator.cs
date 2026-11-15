namespace Main.Entities.Sale;

public sealed record AllocationLot(
	int StorageContentId,
	int ProductId,
	int CurrencyId,
	decimal BuyPrice,
	int Count,
	DateTime PurchaseDatetime);

public sealed record AllocationLine(
	int OriginalIndex,
	int ProductId,
	decimal OriginalPrice,
	decimal DiscountedPrice,
	int Count,
	string? Comment);

public static class SaleContentAllocator
{
	public static List<SaleContent> Distribute(
		IEnumerable<AllocationLot> lots,
		IEnumerable<AllocationLine> lines)
	{
		var result = new List<(int OriginalIndex, SaleContent Content)>();

		var storageByProduct = lots
			.Select(lot =>
			{
				if (lot.Count <= 0)
					throw new InvalidOperationException("Invalid taken quantity");

				return new
				{
					lot.ProductId,
					Detail = SaleContentDetail.Create(
						lot.StorageContentId,
						lot.CurrencyId,
						lot.BuyPrice,
						lot.Count,
						lot.PurchaseDatetime)
				};
			})
			.GroupBy(x => x.ProductId)
			.ToDictionary(
				group => group.Key,
				group => group
					.Select(x => x.Detail)
					.OrderByDescending(detail => detail.Count)
					.ThenByDescending(detail => detail.BuyPrice)
					.ToList());

		foreach (var line in lines.OrderByDescending(x => x.Count))
		{
			if (!storageByProduct.TryGetValue(line.ProductId, out var storage))
				throw new InvalidOperationException($"No storage for product {line.ProductId}");

			var details = new List<SaleContentDetail>();
			var leftToDistribute = line.Count;
			var index = 0;

			while (leftToDistribute > 0 && index < storage.Count)
			{
				var detail = storage[index];

				if (leftToDistribute >= detail.Count)
				{
					details.Add(detail);
					leftToDistribute -= detail.Count;
					storage.RemoveAt(index);
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

					storage[index] = SaleContentDetail.Create(
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
				line.ProductId,
				line.OriginalPrice,
				line.DiscountedPrice,
				details);

			content.SetComment(line.Comment);

			result.Add((line.OriginalIndex, content));
		}

		return result.OrderBy(x => x.OriginalIndex).Select(x => x.Content).ToList();
	}
}
