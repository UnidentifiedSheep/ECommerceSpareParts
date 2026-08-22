namespace Domain.CommonEntities.DataSlices.Definitions.Scalar;

public class ScalarDataSliceDefinition<TScalar> : 
    DataSliceDefinition
{
    public TScalar Value { get; protected set; }
    
    protected ScalarDataSliceDefinition(
        DataSliceGroup group,
        TScalar scalar) : base(group)
    {
        Value = scalar;
    }
}