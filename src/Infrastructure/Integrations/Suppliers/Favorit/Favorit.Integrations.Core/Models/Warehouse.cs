using System.Text.Json.Serialization;

namespace Favorit.Integrations.Core.Models;

public record Warehouse
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("own")]
    public required bool Own { get; init; } //Is it our(favorit) warehouse.

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("shipmentDate")]
    public required DateTime
        ShipmentDate { get; init; } //Date is shipment, Time till when order can be created.

    [JsonPropertyName("stock")]
    public required int Stock { get; init; }

    [JsonPropertyName("notRefund")]
    public required bool NotRefund { get; init; }
}