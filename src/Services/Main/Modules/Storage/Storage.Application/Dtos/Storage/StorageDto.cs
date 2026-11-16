using System.Text.Json.Serialization;
using Main.Entities.Storage.Enums;

namespace Main.Modules.Storage.Application.Dtos.Storage;

public record StorageDto
{
	[JsonPropertyName("code")]
	public required string Code { get; init; }

	[JsonPropertyName("description")]
	public required string? Description { get; init; }

	[JsonPropertyName("location")]
	public required string? Location { get; init; }

	[JsonPropertyName("type")]
	public required StorageType Type { get; init; }
}
