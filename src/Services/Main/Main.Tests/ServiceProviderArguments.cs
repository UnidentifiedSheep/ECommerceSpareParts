using Tests.Interfaces.ServiceProvider;

namespace Tests;

public record ServiceProviderArguments : IServiceProviderArgument
{
    public required string PgsqlConnectionString { get; init; }
    public required string CacheConnectionString { get; init; }
}