namespace Abstractions.Models;

public class TypedSetting<T> : TypedSetting
{
    public override Type Type { get; } = typeof(T);
    public override string Key { get; }
    public override object FallbackValue { get; }

    public TypedSetting(string key, T fallbackValue)
    {
        if (fallbackValue == null) throw new ArgumentNullException(nameof(fallbackValue));
        Key = key;
        FallbackValue = fallbackValue;
    }
}

public abstract class TypedSetting
{
    public abstract Type Type { get; }
    public abstract string Key { get; }
    public abstract object FallbackValue { get; }
}