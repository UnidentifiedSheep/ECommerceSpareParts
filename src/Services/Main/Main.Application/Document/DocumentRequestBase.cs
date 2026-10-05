using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;
using Main.Entities;
using Main.Enums.Documents;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Document;

public abstract record DocumentRequestBase : IDocumentRequest
{
	[JsonPropertyName("documentType")]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(nameof(DocumentType))]
	[SchemaFieldLabel(DocumentRequestTypeNameMessage.Key)]
	[SchemaFieldDescription(DocumentRequestTypeDescriptionMessage.Key)]
	public required DocumentType DocumentType { get; init; }

	[JsonPropertyName("culture")]
	[SchemaInputControl(InputControlType.TextField)]
	[RequiredSchemaField]
	[SchemaFieldLabel(DocumentRequestCultureNameMessage.Key)]
	[SchemaFieldDescription(DocumentRequestCultureDescriptionMessage.Key)]
	public string Culture { get; init; } = "en";
}
