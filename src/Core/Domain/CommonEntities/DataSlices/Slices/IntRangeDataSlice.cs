using Domain.CommonEntities.DataSlices.Definitions.Range;

namespace Domain.CommonEntities.DataSlices.Slices;

public abstract class IntRangeDataSlice
    : RangeDataSlice<int, IntRangeDataSliceDefinition>
{
    protected IntRangeDataSlice(
        IntRangeDataSliceDefinition definition,
        int value,
        string? payload = null)
        : base(definition, value, payload)
    {
    }
}
