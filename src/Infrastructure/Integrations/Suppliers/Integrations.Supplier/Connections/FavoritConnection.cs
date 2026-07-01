namespace Integrations.Supplier.Connections;

public record FavoritConnection
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
}