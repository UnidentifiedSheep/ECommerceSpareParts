namespace Main.Application;

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
        public static string GetUserCacheKey(Guid userId)
        {
            return $"user:{userId}";
        }

        public static string GetUserDiscountCacheKey(Guid userId)
        {
            return $"user:{userId}:discount";
        }

        public static string GetUserRolesAndPermissionsCacheKey(Guid userId)
        {
            return $"user:{userId}:roles:permissions";
        }
    }
}