namespace Main.Modules.Product.Application.Static;

public static class ProductCacheKeys
{
	public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

	public static string Product(int id) => $"product:{id}";

	public static string ProductCrosses(int id) => $"product:{id}:crosses";

	public static string ProductCrossRelations(int id) => $"product:{id}:crosses:relations";

	public static string ProductSizes(int id) => $"product:{id}:sizes";

	public static string ProductWeight(int id) => $"product:{id}:weight";
}
