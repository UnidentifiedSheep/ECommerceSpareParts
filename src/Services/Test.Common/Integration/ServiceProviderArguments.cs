using Tests.Interfaces.ServiceProvider;

namespace Tests.Integration;

internal sealed record ServiceProviderArguments : IServiceProviderArgument
{
    public required string PgsqlConnectionString { get; init; }
}
