using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Producer;

public record InternalFullProducer
{
    [JsonPropertyName("producer")]
    public required InternalProducer Producer { get; init; }

    [JsonPropertyName("aliases")]
    public required IReadOnlyList<InternalProducerAlias> Aliases { get; init; }
}