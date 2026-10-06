using Locan.Core.Interfaces;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions.Base.Localized;

public abstract class LocalizedBadRequestException : BadRequestException, ILocalizableException
{
	protected LocalizedBadRequestException(ILocalizableMessage message) : base(null)
	{
		LocalizableMessage = message;
	}

	protected LocalizedBadRequestException(ILocalizableMessage message, object relatedData) : base(
		null,
		relatedData)
	{
		LocalizableMessage = message;
	}

	protected LocalizedBadRequestException(ILocalizableMessage message, string details) : base(null, details)
	{
		LocalizableMessage = message;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
