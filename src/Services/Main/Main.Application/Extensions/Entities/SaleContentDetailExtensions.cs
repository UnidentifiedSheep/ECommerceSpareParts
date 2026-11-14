using Main.Application.Models;
using Main.Entities.Sale;

namespace Main.Application.Extensions.Entities;

public static class SaleContentDetailExtensions
{
	public static RestoreContentItem ToRestoreContentItem(this SaleContentDetail detail, int productId)
		=> new(
			detail.StorageContentId,
			productId,
			detail.CurrencyId,
			detail.BuyPrice,
			detail.Count);
}
