using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Producer;

public record InternalProducer
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}
