namespace Domain.CommonEntities.DataSlices.Definitions.Range;

public class DateRangeDataSliceDefinition : DataSliceRangeDefinition<DateTime>
{
    public DateRangeDataSliceDefinition(
        DataSliceGroup group, 
        DateTime rangeStart,
        DateTime rangeEnd
        ) : base(group, rangeStart, rangeEnd)
    {
        
    }
}