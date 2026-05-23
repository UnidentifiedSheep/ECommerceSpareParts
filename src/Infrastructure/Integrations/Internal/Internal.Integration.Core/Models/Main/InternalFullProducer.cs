using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalFullProducer
{
    [JsonPropertyName("producer")]
    public required InternalProducer Producer { get; init; }

    [JsonPropertyName("otherNames")]
    public required IReadOnlyList<InternalProducerOtherName> OtherNames { get; init; }
}

public record InternalProducer
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }
}

public record InternalProducerOtherName
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }

    [JsonPropertyName("otherName")]
    public required string OtherName { get; init; }

    [JsonPropertyName("whereUsed")]
    public required string WhereUsed { get; init; }
}
