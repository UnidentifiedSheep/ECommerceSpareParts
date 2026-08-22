using Domain.CommonEntities.DataSlices.Definitions.Range;

namespace Domain.CommonEntities.DataSlices.Slices;

public class DateRangeDataSlice : RangeDataSlice<DateTime, DateRangeDataSliceDefinition>
{
    protected DateRangeDataSlice(
        DateRangeDataSliceDefinition definition,
        DateTime value,
        string? payload = null)
        : base(definition, value, payload)
    {
    }
}