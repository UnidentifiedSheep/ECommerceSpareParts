using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Producer;

public record ProducerFullDto : ProducerDto
{
	[JsonPropertyName("aliases")]
	public required IReadOnlyCollection<string> Aliases { get; init; }
}
