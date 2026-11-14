namespace Main.Application.Models;

public record RestoreContentItem(
	int StorageContentId,
	int ProductId,
	int CurrencyId,
	decimal BuyPrice,
	int Count);
