using Locan.Core.Interfaces;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions.Base.Localized;

public abstract class LocalizedInternalServerException : InternalServerException, ILocalizableException
{
	protected LocalizedInternalServerException(ILocalizableMessage message) : base(message.MessageKey)
	{
		LocalizableMessage = message;
	}

	protected LocalizedInternalServerException(ILocalizableMessage message, string details) : base(
		message.MessageKey,
		details)
	{
		LocalizableMessage = message;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
