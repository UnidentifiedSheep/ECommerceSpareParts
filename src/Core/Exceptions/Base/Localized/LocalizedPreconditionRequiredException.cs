using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base.Localized;

public abstract class LocalizedPreconditionRequiredException : PreconditionRequiredException, ILocalizableException
{
    protected LocalizedPreconditionRequiredException(
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
