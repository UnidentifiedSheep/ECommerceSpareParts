namespace Domain.CommonEntities.DataSlices.Definitions.Range;

public abstract class DateRangeDataSliceDefinition : DataSliceRangeDefinition<DateTime>
{
    protected DateRangeDataSliceDefinition(
        DataSliceGroup group,
        DateTime rangeStart,
        DateTime rangeEnd)
        : base(group, rangeStart, rangeEnd) { }
}
