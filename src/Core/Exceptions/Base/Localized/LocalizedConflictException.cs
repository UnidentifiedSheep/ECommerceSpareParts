using Abstractions.Interfaces.Exceptions;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedConflictException : ConflictException, ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; }
	protected LocalizedConflictException(ILocalizableMessage message) : base(null)
	{
		LocalizableMessage = message;
	}

	protected LocalizedConflictException(
		ILocalizableMessage message,
		object relatedData) : base(null, relatedData)
	{
		LocalizableMessage = message;
	}

	protected LocalizedConflictException(
		ILocalizableMessage message,
		string details) : base(null, details)
	{
		LocalizableMessage = message;
	}
}
