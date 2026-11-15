namespace Main.Modules.Sale.Abstractions;

public sealed record SaleContentSnapshot(
	int SaleContentId,
	Guid SaleId,
	Guid OrganizationId,
	Guid BuyerId,
	DateTime SaleDatetime,
	int CurrencyId,
	bool IsCompleted,
	int Count,
	decimal OriginalPrice,
	decimal TotalSum,
	IReadOnlyList<SaleContentDetailSnapshot> Details);

public sealed record SaleContentDetailSnapshot(
	int CurrencyId,
	decimal BuyPrice,
	int Count,
	DateTime PurchaseDatetime);
