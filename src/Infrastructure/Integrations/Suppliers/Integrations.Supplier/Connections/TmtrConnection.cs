namespace Integrations.Supplier.Connections;

public record TmtrConnection
{
    public required string BaseUrl { get; init; }
    public required string Login { get; init; }
    public required string Password { get; init; }
}