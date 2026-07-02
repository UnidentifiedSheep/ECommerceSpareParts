using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Producer;

public record InternalFullProducer : InternalProducer
{
    [JsonPropertyName("aliases")]
    public required IReadOnlyList<string> Aliases { get; init; }
}