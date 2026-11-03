namespace Search.Application.Models.ProductUpdates;

public sealed record ProductStockUpdate(
	int ProductId,
	IReadOnlyDictionary<string, int> Stocks,
	DateTime UpdatedAt,
	bool IsSnapshot);
