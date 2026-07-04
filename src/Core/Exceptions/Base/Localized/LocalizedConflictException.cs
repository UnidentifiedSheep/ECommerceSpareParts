using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base.Localized;

public abstract class LocalizedConflictException : ConflictException, ILocalizableException
{
    protected LocalizedConflictException(
        string messageKey,
        object[]? arguments = null)
        : base(null)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedConflictException(
        string messageKey,
        object relatedData,
        object[]? arguments = null)
        : base(null, relatedData)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedConflictException(
        string messageKey,
        string details,
        object[]? arguments = null)
        : base(null, details)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}