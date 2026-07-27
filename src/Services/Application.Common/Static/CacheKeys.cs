namespace Application.Common.Static;

public static class CacheKeys
{
    public static class SettingsCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromMinutes(5);

        public static string FavoritSettings => "favorit-settings";
        public static string TmtrConnection => "tmtr-connection";
        public static string TmtrSettings => "tmtr-settings";
    }
}
