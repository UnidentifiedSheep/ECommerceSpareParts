using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;
using Exceptions;

namespace Analytics.Application.Lrts.MarkupCalculation;

public record MarkupCalculationState
{
    [JsonPropertyName("rangeStart")]
    public DateTime? RangeStart { get; init; }
    
    [JsonPropertyName("rangeEnd")]
    public DateTime? RangeEnd { get; init; }
}

public record MarkupCalculationInputState : IInputState
{
    [InputControl(InputControlType.DatePicker)]
    [LocalizedJsonFieldDescription("datetime.range.start.description")]
    [LocalizedJsonFieldName("datetime.range.start.name")]
    [JsonPropertyName("rangeStart")]
    public DateTime? RangeStart { get; init; }
    
    [InputControl(InputControlType.DatePicker)]
    [LocalizedJsonFieldDescription("datetime.range.end.description")]
    [LocalizedJsonFieldName("datetime.range.end.name")]
    [JsonPropertyName("rangeEnd")]
    public DateTime? RangeEnd { get; init; }

    public static string GetAndValidateState(string jsonState)
    {
        var obj = JsonSerializer.Deserialize<MarkupCalculationState>(jsonState)
            ?? throw new InvalidInputException("markup.calculation.input.invalid");

        if (obj.RangeStart is not null && obj.RangeEnd is not null && obj.RangeStart > obj.RangeEnd)
            throw new InvalidInputException("markup.calculation.range.start.must.be.before.or.equal.end");
        
        return jsonState;
    }
}
