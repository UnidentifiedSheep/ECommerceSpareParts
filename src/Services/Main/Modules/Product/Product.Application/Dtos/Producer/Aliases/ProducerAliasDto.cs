using System.Text.Json.Serialization;

namespace Main.Modules.Product.Application.Dtos.Producer.Aliases;

public record ProducerAliasDto
{
	[JsonPropertyName("producerId")]
	public required int ProducerId { get; init; }

	[JsonPropertyName("alias")]
	public required string Alias { get; init; }
}
