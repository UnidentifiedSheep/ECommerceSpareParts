namespace Core.Models;

public class Versioned<T>
{
    public Versioned(T value)
    {
        Value = value;
        Version = DateTime.UtcNow.Ticks;
    }

    public T Value { get; set; }
    public long Version { get; set; }
}