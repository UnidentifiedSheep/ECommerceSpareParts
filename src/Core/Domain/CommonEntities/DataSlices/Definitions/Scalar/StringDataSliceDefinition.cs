namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public class StringDataSliceDefinition : ScalarDataSliceDefinition<string>
{
    public StringDataSliceDefinition(
        DataSliceGroup group, 
        string scalar
        ) : base(group, scalar) { }
}