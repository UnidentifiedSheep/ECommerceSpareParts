using Test.Common.Interfaces.ServiceProvider;

namespace Analytics.Integration.Tests;

public record ServiceProviderArguments : IServiceProviderArgument
{
    public required string PgsqlConnectionString { get; init; }
    public required string CacheConnectionString { get; init; }
}