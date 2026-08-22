namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public abstract class StringDataSliceDefinition : ScalarDataSliceDefinition<string>
{
    protected StringDataSliceDefinition(
        DataSliceGroup group,
        string value)
        : base(group, value) { }
}
