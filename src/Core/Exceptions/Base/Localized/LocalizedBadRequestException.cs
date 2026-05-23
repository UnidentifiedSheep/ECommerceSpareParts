using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base.Localized;

public abstract class LocalizedBadRequestException : BadRequestException, ILocalizableException
{
    protected LocalizedBadRequestException(
        string messageKey,
        object[]? arguments = null)
        : base(null)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedBadRequestException(
        string messageKey,
        object relatedData,
        object[]? arguments = null)
        : base(null, relatedData)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    protected LocalizedBadRequestException(
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
