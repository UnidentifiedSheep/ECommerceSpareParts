using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using Pricing.Entities;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Pricing.Application.Lrts.PriceCandidateCalculation;

public class PriceCandidateCalculationState : IInputState
{
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[RequiredSchemaField]
	[SchemaFieldDescription(LrtPriceCandidateCalculationProductIdDescriptionMessage.Key)]
	[SchemaFieldLabel(LrtPriceCandidateCalculationProductIdNameMessage.Key)]
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Storage, "code")]
	[RequiredSchemaField]
	[SchemaFieldDescription(LrtPriceCandidateCalculationStorageCodeDescriptionMessage.Key)]
	[SchemaFieldLabel(LrtPriceCandidateCalculationStorageCodeNameMessage.Key)]
	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }

	public void ValidateState()
	{
	}
}
