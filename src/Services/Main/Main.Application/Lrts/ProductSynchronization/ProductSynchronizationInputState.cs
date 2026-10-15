using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Main.Application.Interfaces.Products;
using Main.Enums;

namespace Main.Application.Lrts.ProductSynchronization;

public sealed record ProductSynchronizationInputState : IInputState
{
	[JsonPropertyName("parts")]
	public ProductEventParts Parts { get; init; } = ProductEventParts.All;

	public void ValidateState()
	{
		if (Parts == 0 || (Parts & ~ProductEventParts.All) != 0)
			throw new InvalidOperationException("At least one supported product event part must be selected.");
	}
}
