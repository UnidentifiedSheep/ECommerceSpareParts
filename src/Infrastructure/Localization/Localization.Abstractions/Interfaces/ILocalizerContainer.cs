using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface ILocalizerContainer
{
    Locale Locale { get; }
    IReadOnlyDictionary<string, string> KetMessages { get; }
    void Initialize(Dictionary<string, string> ketMessages);
}