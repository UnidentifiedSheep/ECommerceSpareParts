namespace Localization.Abstractions.Interfaces;

public interface IScopedLocalizedJsonSerializer
{
    string Serialize<T>(T value);
}