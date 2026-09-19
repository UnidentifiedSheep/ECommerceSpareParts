using Abstractions.Interfaces.Exceptions;
using Abstractions.Models.Validation;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedValidationException : ValidationException, ILocalizableException
{
	protected LocalizedValidationException(
		IEnumerable<ValidationErrorModel> errors,
		ILocalizableMessage message) : base(errors)
	{
		LocalizableMessage = message;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
