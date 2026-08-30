namespace Extensions;

public static class TypeExtensions
{
	public static Type GetGenericTypeDefinitionOrSelf(this Type type) =>
		type.IsGenericType ? type.GetGenericTypeDefinition() : type;
}
