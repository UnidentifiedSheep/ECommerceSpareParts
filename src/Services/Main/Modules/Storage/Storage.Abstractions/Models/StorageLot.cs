namespace Main.Modules.Storage.Abstractions.Models;

public record StorageLot(
	int Id,
	int ProductId,
	int CurrencyId,
	decimal BuyPrice,
	int Count,
	DateTime PurchaseDatetime);
