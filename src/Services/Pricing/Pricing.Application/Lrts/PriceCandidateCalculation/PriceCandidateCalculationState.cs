using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Pricing.Application.Lrts.PriceCandidateCalculation;

public class PriceCandidateCalculationState : IInputState
{
    [SchemaInputControl(InputControlType.EntitySelector)]
    [SchemaDependsOnEntity("Product", "id")]
    [RequiredSchemaField]
    [SchemaFieldDescription("lrt.price.candidate.calculation.product.id.description")]
    [SchemaFieldLabel("lrt.price.candidate.calculation.product.id.name")]
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [SchemaInputControl(InputControlType.EntitySelector)]
    [SchemaDependsOnEntity("Storage", "name")]
    [RequiredSchemaField]
    [SchemaFieldDescription("lrt.price.candidate.calculation.storage.name.description")]
    [SchemaFieldLabel("lrt.price.candidate.calculation.storage.name.name")]
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    public void ValidateState() { }
}
