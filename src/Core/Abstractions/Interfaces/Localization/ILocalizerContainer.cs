namespace Abstractions.Interfaces.Localization;

public interface ILocalizerContainer
{
    string Locale { get; }
    IReadOnlyDictionary<string, string> KetMessages { get; }
    void Initialize(Dictionary<string, string> ketMessages);
}