using Tests.Interfaces.ServiceProvider;

namespace Search.Tests;

public sealed record ServiceProviderArguments : IServiceProviderArgument
{
	public required Uri OpenSearchEndpoint { get; init; }
	public required string IndexPrefix { get; init; }
	public required string RedisConnectionString { get; init; }
}
