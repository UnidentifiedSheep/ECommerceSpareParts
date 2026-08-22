namespace Domain.CommonEntities.DataSlices.Definitions.Range;

public abstract class DataSliceRangeDefinition<TRange> : 
    DataSliceDefinition 
    where TRange : IComparable<TRange>, IEquatable<TRange>
{
    public TRange RangeStart { get; protected set; }
    public TRange RangeEnd { get; protected set; }

    protected DataSliceRangeDefinition(
        DataSliceGroup group,
        TRange rangeStart,
        TRange rangeEnd) : base(group)
    {
        if (rangeStart.CompareTo(rangeEnd) >= 0)
            throw new InvalidOperationException(
                "Range start must be smaller than range end.");
        
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
    }
    
    public bool Contains(TRange value) =>
        value.CompareTo(RangeStart) >= 0 &&
        value.CompareTo(RangeEnd) < 0;
}