using System.Text.Json.Serialization;
using Application.Common.Models;
using Main.Entities.Storage.Enums;

namespace Main.Modules.Storage.Application.Dtos.Storage;

public record PatchStorageDto
{
	[JsonPropertyName("description")]
	public PatchField<string?> Description { get; init; } = PatchField<string?>.NotSet();

	[JsonPropertyName("location")]
	public PatchField<string?> Location { get; init; } = PatchField<string?>.NotSet();

	[JsonPropertyName("type")]
	public PatchField<StorageType> Type { get; init; } = PatchField<StorageType>.NotSet();
}
