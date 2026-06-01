namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DependencyJsonPropertyAttribute : Attribute
{
    public string DependsOnEntity { get; }
    
    public DependencyJsonPropertyAttribute(string dependsOnEntity)
    {
        DependsOnEntity = dependsOnEntity;
    }
}