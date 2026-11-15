using Application.Common.Querying;
using Main.Entities.Purchase;

namespace Main.Application.Configs;

public static class PurchaseSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<Purchase, DateTime>(x => x.PurchaseDatetime, true)
			.Map<Purchase, DateTime>("dateTime", x => x.PurchaseDatetime)
			.Map<Purchase, decimal>("totalSum", x => x.Contents.Sum(content => content.TotalSum))
			.Map<Purchase, Guid>("id", x => x.Id);
}
