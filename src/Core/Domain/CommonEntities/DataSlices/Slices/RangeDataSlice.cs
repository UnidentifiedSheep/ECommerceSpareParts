using Domain.CommonEntities.DataSlices.Definitions.Range;

namespace Domain.CommonEntities.DataSlices.Slices;

public abstract class RangeDataSlice<TValue, TDefinition> : DataSlice
    where TValue : IComparable<TValue>, IEquatable<TValue>
    where TDefinition : DataSliceRangeDefinition<TValue>
{
    public TValue Value { get; private set; }

    protected RangeDataSlice(
        TDefinition definition,
        TValue value,
        string? payload = null)
        : base(definition, payload)
    {
        if (!definition.Contains(value))
            throw new InvalidOperationException(
                "Slice value must belong to the definition range.");

        Value = value;
    }
}