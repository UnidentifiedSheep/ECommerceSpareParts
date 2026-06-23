using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Producer;

public record InternalProducerOtherName
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }

    [JsonPropertyName("otherName")]
    public required string OtherName { get; init; }

    [JsonPropertyName("whereUsed")]
    public required string WhereUsed { get; init; }
}
