namespace Abstractions.Interfaces.Localization;

public interface IScopedStringLocalizer : IDisposable
{
    void SetLocale(string locale);
    string Get(string key);
    string this[string key] { get; }
}