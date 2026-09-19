using System.Text.Json.Serialization;
using Analytics.Entities;
using Application.Common.Interfaces.Lrt;
using Exceptions;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Analytics.Application.Lrts.MarkupCalculation;

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
