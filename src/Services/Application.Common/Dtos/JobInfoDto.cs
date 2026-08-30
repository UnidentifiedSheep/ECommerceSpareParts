using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Models;

namespace Application.Common.Dtos;

public record JobInfoDto
{
	[JsonPropertyName("systemName")]
	public required string SystemName { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("initStateSchema")]
	public required ObjectSchema InitStateSchema { get; init; }
}
