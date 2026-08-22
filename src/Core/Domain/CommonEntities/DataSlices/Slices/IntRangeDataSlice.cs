using Domain.CommonEntities.DataSlices.Definitions.Range;

namespace Domain.CommonEntities.DataSlices.Slices;

public class IntRangeDataSlice
    : RangeDataSlice<int, IntRangeDataSliceDefinition>
{
    public IntRangeDataSlice(
        IntRangeDataSliceDefinition definition,
        int value,
        string? payload = null)
        : base(definition, value, payload)
    {
    }
}