namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public abstract class IntDataSliceDefinition : ScalarDataSliceDefinition<int>
{
    protected IntDataSliceDefinition(
        DataSliceGroup group,
        int value)
        : base(group, value) { }
}
