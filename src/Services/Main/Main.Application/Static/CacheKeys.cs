using Main.Enums;

namespace Main.Application.Static;

public static class CacheKeys
{
	public static class ProductCache
	{
		public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

		public static string Product(int id) => $"product:{id}";

		public static string ProductCrosses(int id, IEnumerable<string>? sortBy) =>
			$"product:{id}:crosses:{string.Join(',', sortBy ?? [])}";

		public static string ProductCrossRelations(int id) => $"product:{id}:crosses:relations";

		public static string ProductSizes(int id) => $"product:{id}:sizes";

		public static string ProductWeight(int id) => $"product:{id}:weight";
	}

	public static class UserCache
	{
		public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

		public static string User(Guid userId) => $"user:{userId}";

		public static string UserDiscount(Guid userId) => $"user:{userId}:discount";

		public static string UserRolesAndPermissions(Guid userId) => $"user:{userId}:roles:permissions";

		public static string RolesAndPermissionsRelations() => "users:roles:permissions:relations";
	}

	public static class CurrencyCache
	{
		public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

		public static string Currency(int currencyId) => $"currency:{currencyId}";

		public static string AllCurrencies() => "currencies";

		public static string CurrencyRate(int currencyId) => $"currency:{currencyId}:rate";
	}

	public static class OneTimeTokenCache
	{
		public static string OneTimeToken(OneTimeTokenPurpose purpose, Guid tokenId) =>
			$"ont-time-token:{purpose}:{tokenId}";
	}
}
