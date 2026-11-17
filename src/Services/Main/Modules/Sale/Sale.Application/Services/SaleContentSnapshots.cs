using Main.Entities.Sale.Enums;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Abstractions;

namespace Main.Modules.Sale.Application.Services;

public static class SaleContentSnapshots
{
	public static SaleContentSnapshot From(SaleContent content) => From(content, content.Sale);

	public static SaleContentSnapshot From(SaleContent content, Entities.Sale.Sale.Sale sale)
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
