using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;

namespace Pricing.Application.Lrts.PriceCandidateCalculationLrt;

public class PriceCandidateCalculationState : IInputState
{
    [InputControl(InputControlType.EntitySelector)]
    [DependsOnEntity("Product", "id")]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("lrt.price.candidate.calculation.product.id.description")]
    [LocalizedJsonFieldName("lrt.price.candidate.calculation.product.id.name")]
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [InputControl(InputControlType.EntitySelector)]
    [DependsOnEntity("Storage", "name")]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("lrt.price.candidate.calculation.storage.name.description")]
    [LocalizedJsonFieldName("lrt.price.candidate.calculation.storage.name.name")]
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    public void ValidateState() { }
}
