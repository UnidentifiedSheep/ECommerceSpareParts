using Abstractions.Interfaces.Exceptions;
using Abstractions.Models.Validation;

namespace Exceptions.Base.Localized;

public abstract class LocalizedValidationException : ValidationException, ILocalizableException
{
    protected LocalizedValidationException(
        IEnumerable<ValidationErrorModel> errors,
        string messageKey,
        object[]? arguments = null)
        : base(errors)
    {
        MessageKey = messageKey;
        Arguments = arguments;
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}