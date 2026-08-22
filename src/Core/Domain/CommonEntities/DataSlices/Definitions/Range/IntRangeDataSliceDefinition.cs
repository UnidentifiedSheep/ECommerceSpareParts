namespace Domain.CommonEntities.DataSlices.Definitions.Range;

public abstract class IntRangeDataSliceDefinition : DataSliceRangeDefinition<int>
{
    protected IntRangeDataSliceDefinition(
        DataSliceGroup group,
        int rangeStart,
        int rangeEnd)
        : base(group, rangeStart, rangeEnd) { }
}
