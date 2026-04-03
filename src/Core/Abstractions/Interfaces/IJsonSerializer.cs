namespace Abstractions.Interfaces;

public interface IJsonSerializer
{
    string Serialize<TValue>(TValue value);
    string Serialize(object value);
    
    TValue? Deserialize<TValue>(string value);
    object? Deserialize(string value, Type type);
}