using Abstractions.Interfaces.Localization;

namespace Localization;

public class LocalizerContainer(string locale) : ILocalizerContainer
{
    private Dictionary<string, string> _ketMessages = new();
    public string Locale { get; } = locale.ToUpperInvariant();
    public IReadOnlyDictionary<string, string> KetMessages => _ketMessages;

    private bool _initialized;
    /// <summary>
    /// Inits ket messages. This method can be executed once.
    /// </summary>
    public void Initialize(Dictionary<string, string> ketMessages)
    {
        if (_initialized)
            throw new InvalidOperationException("The localizer container has already initialized.");
        _ketMessages = ketMessages.ToDictionary();
        _initialized = true;
    }
}