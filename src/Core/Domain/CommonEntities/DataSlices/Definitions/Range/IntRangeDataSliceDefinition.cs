namespace Domain.CommonEntities.DataSlices.Definitions.Range;

public class IntRangeDataSliceDefinition : DataSliceRangeDefinition<int>
{
    public IntRangeDataSliceDefinition(
        DataSliceGroup group, 
        int rangeStart,
        int rangeEnd) : base(group, rangeStart, rangeEnd)
    {
        
    }
}