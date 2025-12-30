using StackExchange.Redis;

namespace Redis;

public static class Redis
{
    private static readonly Dictionary<string, Lazy<IConnectionMultiplexer>> Multiplexers = [];
    public static bool IsConfigured { get; private set; }

    public static void Configure(string configurationString)
    {
        if (IsConfigured) throw new InvalidOperationException("Cannot set pool manager twice");
        IsConfigured = true;
        Multiplexers.Add("default", new Lazy<IConnectionMultiplexer>(() =>
        {
            var options = ConfigurationOptions.Parse(configurationString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        }));
    }

    public static void Configure(Dictionary<string, string> configurationStrings)
    {
        if (IsConfigured) throw new InvalidOperationException("Cannot set pool manager twice");
        IsConfigured = true;
        foreach (var (name,  configurationString) in configurationStrings)
            Multiplexers.Add(name, new Lazy<IConnectionMultiplexer>(() =>
            {
                var options = ConfigurationOptions.Parse(configurationString);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            }));
    }

    public static IDatabase GetRedis(string name = "default")
    {
        if (!IsConfigured) throw new InvalidOperationException("Redis is not configured");
        return Multiplexers[name].Value.GetDatabase();
    }

    public static async Task CloseAllConnections(CancellationToken token = default)
    {
        var keysToRemove = new List<string>();
        foreach (var (key, connection) in Multiplexers)
        {
            await connection.Value.CloseAsync();
            keysToRemove.Add(key);
            if (token.IsCancellationRequested) break;
        }

        foreach (var key in keysToRemove)
            Multiplexers.Remove(key);
    }
}