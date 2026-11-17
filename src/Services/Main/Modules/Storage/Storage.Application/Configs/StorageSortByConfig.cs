using Application.Common.Querying;
using Main.Entities.Storage.Enums;
using Main.Entities.Storage.Storage;

namespace Main.Modules.Storage.Application.Configs;

public static class StorageSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<StorageContent, int>(x => x.Id)
			.Map<StorageContent, int>("id", x => x.Id)
			.Map<StorageContent, int>("productId", x => x.ProductId)
			.Map<StorageContent, string>("storageCode", x => x.StorageCode)
			.Map<StorageContent, int>("count", x => x.Count)
			.Map<StorageContent, decimal>("buyPrice", x => x.BuyPriceInBaseCurrency)
			.Map<StorageContent, DateTime>("purchaseDatetime", x => x.PurchaseDatetime)
			.Map<StorageContent, int>("currencyId", x => x.CurrencyId)
			.Map<StorageContent, long>(
				"availableCount",
				content => Math.Max(
					0L,
					content.Count - content.Locks
						.Where(item => item.State == StorageContentLockState.Active &&
							(item.ExpiresAt == null || item.ExpiresAt > DateTime.UtcNow))
						.Sum(item => (long)item.Quantity)));
}
