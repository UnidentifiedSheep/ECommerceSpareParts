using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public class LocalizerContainer(string locale) : ILocalizerContainer
{
    private bool _initialized;
    private Dictionary<string, string> _ketMessages = new();
    public Locale Locale { get; } = locale;
    public IReadOnlyDictionary<string, string> KetMessages => _ketMessages;

    /// <summary>
    ///     Inits ket messages. This method can be executed once.
    /// </summary>
    public void Initialize(Dictionary<string, string> ketMessages)
    {
        if (_initialized)
            throw new InvalidOperationException("The localizer container has already initialized.");
        _ketMessages = ketMessages.ToDictionary();
        _initialized = true;
    }
}