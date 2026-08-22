namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public class IntDataSliceDefinition : ScalarDataSliceDefinition<int>
{
    public IntDataSliceDefinition(
        DataSliceGroup group,
        int value
        ) : base(group, value) { }
}