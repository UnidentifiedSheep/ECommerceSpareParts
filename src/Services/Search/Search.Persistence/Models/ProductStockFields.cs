namespace Search.Persistence.Models;

public static class ProductStockFields
{
	public static string GetStorageKey(string storageCode) =>
		Uri.EscapeDataString(storageCode).Replace(".", "%2E", StringComparison.Ordinal);
}
