using Locan.Core.Interfaces;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions.Base.Localized;

public abstract class LocalizedConflictException : ConflictException, ILocalizableException
{
	protected LocalizedConflictException(ILocalizableMessage message) : base(null)
	{
		LocalizableMessage = message;
	}

	protected LocalizedConflictException(ILocalizableMessage message, object relatedData) : base(
		null,
		relatedData)
	{
		LocalizableMessage = message;
	}

	protected LocalizedConflictException(ILocalizableMessage message, string details) : base(null, details)
	{
		LocalizableMessage = message;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
