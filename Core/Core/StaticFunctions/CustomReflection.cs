using System.Reflection;

namespace Core.StaticFunctions;

public static class CustomReflection
{
    public static T SetupValuesViaNames<T>(this T insertionClass, Dictionary<string, string> values)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var pr in values)
        {
            var property = properties.FirstOrDefault(p => p.Name == pr.Key);
            if (property == null) continue;
            property.SetValue(insertionClass, System.Convert.ChangeType(pr.Value, property.PropertyType));
        }
        return insertionClass;
    }
}