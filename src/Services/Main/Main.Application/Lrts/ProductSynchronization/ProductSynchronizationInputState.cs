using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Main.Enums;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Lrts.ProductSynchronization;

public sealed record ProductSynchronizationInputState : IInputState
{
	[JsonPropertyName("parts")]
	[JsonConverter(typeof(JsonStringEnumConverter<ProductEventParts>))]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(nameof(ProductEventParts))]
	[SchemaFieldLabel("lrt.product.synchronization.parts.name")]
	[SchemaFieldDescription("lrt.product.synchronization.parts.description")]
	public ProductEventParts Parts { get; init; } = ProductEventParts.All;

	public void ValidateState()
	{
		if (Parts == 0 || (Parts & ~ProductEventParts.All) != 0)
			throw new InvalidOperationException("At least one supported product event part must be selected.");
	}
}
