using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Exceptions;
using Main.Entities;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Analytics.Lrts.MarkupCalculation;

public record MarkupCalculationState : MarkupCalculationInputState;

public record MarkupCalculationInputState : IInputState
{
	[SchemaInputControl(InputControlType.DatePicker)]
	[JsonPropertyName("rangeStart")]
	public DateTime? RangeStart { get; init; }

	[SchemaInputControl(InputControlType.DatePicker)]
	[JsonPropertyName("rangeEnd")]
	public DateTime? RangeEnd { get; init; }

	public void ValidateState()
	{
		if (RangeStart is not null && RangeEnd is not null && RangeStart > RangeEnd)
			throw new InvalidInputException(
				MarkupCalculationRangeStartMustBeBeforeOrEqualEndMessage.Instance);
	}
}
