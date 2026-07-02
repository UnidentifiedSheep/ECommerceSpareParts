using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Producer;

public record ProducerFullDto : ProducerDto
{
    [JsonPropertyName("aliases")]
    public required IEnumerable<string> Aliases { get; init; }
}