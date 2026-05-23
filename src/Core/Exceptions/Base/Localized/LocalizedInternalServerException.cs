using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base.Localized;

public abstract class LocalizedInternalServerException : InternalServerException, ILocalizableException
{
    protected LocalizedInternalServerException(
        string messageKey,
        object[]? arguments = null)
        : base(messageKey)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedInternalServerException(
        string messageKey,
        string details,
        object[]? arguments = null)
        : base(messageKey, details)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}
