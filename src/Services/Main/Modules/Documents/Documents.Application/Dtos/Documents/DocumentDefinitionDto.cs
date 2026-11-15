using System.Text.Json.Serialization;
using Main.Enums.Documents;
using SchemaGeneration.Abstractions.Models;

namespace Main.Application.Dtos.Documents;

public sealed record DocumentDefinitionDto
{
	[JsonPropertyName("systemName")]
	public required string SystemName { get; init; }

	[JsonPropertyName("documentGroup")]
	public required string DocumentGroup { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("supportedDocumentTypes")]
	public required IReadOnlyList<DocumentType> SupportedDocumentTypes { get; init; }

	[JsonPropertyName("requestSchema")]
	public required ObjectSchema RequestSchema { get; init; }

	[JsonPropertyName("templateSchema")]
	public required ObjectSchema TemplateSchema { get; init; }
}
