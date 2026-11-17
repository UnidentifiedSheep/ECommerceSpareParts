namespace Main.Modules.Storage.Abstractions.Models;

public record NewStorageLot(int ProductId,
	int CurrencyId,
	decimal BuyPrice,
	int Count,
	DateTime PurchaseDate);

public record StoredLot(
	int Id,
	int ProductId,
	int Count,
	int CurrencyId,
	decimal BuyPrice,
	DateTime PurchaseDatetime);

public record RestoreStorageLot(int StorageContentId,
	int ProductId,
	int CurrencyId,
	decimal BuyPrice,
	int Count);
