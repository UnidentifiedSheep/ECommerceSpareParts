using Application.Common.Querying;

namespace Main.Modules.Purchase.Application.Configs;

public static class PurchaseSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<Entities.Purchase.Purchase.Purchase, DateTime>(x => x.PurchaseDatetime, true)
			.Map<Entities.Purchase.Purchase.Purchase, DateTime>("dateTime", x => x.PurchaseDatetime)
			.Map<Entities.Purchase.Purchase.Purchase, decimal>(
				"totalSum",
				x => x.Contents.Sum(content => content.TotalSum))
			.Map<Entities.Purchase.Purchase.Purchase, Guid>("id", x => x.Id);
}
