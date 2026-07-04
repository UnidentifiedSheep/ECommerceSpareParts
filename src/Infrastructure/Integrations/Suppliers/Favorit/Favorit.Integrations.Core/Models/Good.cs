using System.Text.Json.Serialization;

namespace Favorit.Integrations.Core.Models;

public record Good
{
    [JsonPropertyName("goodsID")]
    public required string Id { get; init; }

    [JsonPropertyName("brand")]
    public required string Brand { get; init; }

    [JsonPropertyName("number")]
    public required string Number { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("count")]
    public required int TotalCount { get; init; } //From all storages.

    [JsonPropertyName("rate")]
    public required int Rate { get; init; }

    [JsonPropertyName("analogues")]
    public IReadOnlyList<Good> Analogues { get; init; } = [];

    [JsonPropertyName("warehouses")]
    public IReadOnlyList<Warehouse> Warehouses { get; init; } = [];

    [JsonPropertyName("notRefund")]
    public required bool NotRefund { get; init; }
}