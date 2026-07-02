using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Producer;

public record InternalProducerAlias
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }

    [JsonPropertyName("alias")]
    public required string Alias { get; init; }
}