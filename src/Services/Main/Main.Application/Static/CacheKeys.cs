namespace Main.Application.Static;

public static class CacheKeys
{
    public static class ProductCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string Product(int id)
        {
            return $"product:{id}";
        }

        public static string ProductCrosses(int id, string? sortBy)
        {
            return $"product:{id}:crosses:{sortBy}";
        }

        public static string ProductCrossRelations(int id)
        {
            return $"product:{id}:crosses:relations";
        }

        public static string ProductSizes(int id)
        {
            return $"product:{id}:sizes";
        }

        public static string ProductWeight(int id)
        {
            return $"product:{id}:weight";
        }
    }
    
    public static class UserCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string User(Guid userId)
        {
            return $"user:{userId}";
        }

        public static string UserDiscount(Guid userId)
        {
            return $"user:{userId}:discount";
        }

        public static string UserRolesAndPermissions(Guid userId)
        {
            return $"user:{userId}:roles:permissions";
        }

        public static string RolesAndPermissionsRelations()
        {
            return "users:roles:permissions:relations";
        }
    }

    public static class CurrencyCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string Currency(int currencyId)
        {
            return $"currency:{currencyId}";
        }
        
        public static string AllCurrencies()
        {
            return "currencies";
        }

        public static string CurrencyRate(int currencyId)
        {
            return $"currency:{currencyId}:rate";
        }
    }
}
