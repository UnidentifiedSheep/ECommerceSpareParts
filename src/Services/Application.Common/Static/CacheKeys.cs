namespace Application.Common.Static;

public static class CacheKeys
{
    public static class SettingsCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromMinutes(5);

        public static string FavoritSettings => "favorit-settings";
    }
}