namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public abstract class ScalarDataSliceDefinition<TScalar> :
    DataSliceDefinition
{
    public TScalar Value { get; protected set; } = default!;

    protected ScalarDataSliceDefinition(
        DataSliceGroup group,
        TScalar scalar) : base(group)
    {
        Value = scalar;
    }
}
