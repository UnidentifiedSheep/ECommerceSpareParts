using System.Text.Json.Serialization;
using Application.Common.Models;

namespace Main.Modules.Product.Application.Dtos.Producer;

public record PatchProducerDto
{
	[JsonPropertyName("name")]
	public PatchField<string> Name { get; init; } = PatchField<string>.NotSet();

	[JsonPropertyName("description")]
	public PatchField<string?> Description { get; init; } = PatchField<string?>.NotSet();
}
