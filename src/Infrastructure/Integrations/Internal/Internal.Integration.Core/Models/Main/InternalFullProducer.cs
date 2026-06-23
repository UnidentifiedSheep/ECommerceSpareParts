using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalFullProducer
{
    [JsonPropertyName("producer")]
    public required InternalProducer Producer { get; init; }

    [JsonPropertyName("otherNames")]
    public required IReadOnlyList<InternalProducerOtherName> OtherNames { get; init; }
}
