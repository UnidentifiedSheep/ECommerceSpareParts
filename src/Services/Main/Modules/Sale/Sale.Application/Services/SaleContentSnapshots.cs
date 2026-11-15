using Main.Entities.Sale;
using Main.Enums;
using Main.Modules.Sale.Abstractions;

namespace Main.Application.Services;

public static class SaleContentSnapshots
{
	public static SaleContentSnapshot From(SaleContent content) => From(content, content.Sale);

	public static SaleContentSnapshot From(SaleContent content, Sale sale)
		=> new(
			content.Id,
			content.SaleId,
			sale.OrganizationId,
			sale.UserId,
			sale.SaleDatetime,
			sale.CurrencyId,
			sale.State == SaleState.Completed,
			content.Count,
			content.OriginalPrice,
			content.TotalSum,
			content.Details
				.Select(detail => new SaleContentDetailSnapshot(
					detail.CurrencyId,
					detail.BuyPrice,
					detail.Count,
					detail.PurchaseDatetime))
				.ToArray());
}
