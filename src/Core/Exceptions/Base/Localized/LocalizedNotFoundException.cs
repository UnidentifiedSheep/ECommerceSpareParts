using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base.Localized;

public abstract class LocalizedNotFoundException : NotFoundException, ILocalizableException
{
    protected LocalizedNotFoundException(
        string messageKey,
        object[]? arguments = null)
        : base(null)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedNotFoundException(
        string messageKey,
        object relatedData,
        object[]? arguments = null)
        : base(null, relatedData)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}