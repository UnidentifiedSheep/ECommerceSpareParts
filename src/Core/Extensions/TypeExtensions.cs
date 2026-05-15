namespace Extensions;

public static class TypeExtensions
{
    public static Type GetGenericTypeDefinitionOrSelf(this Type type)
    {
        return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    }
}