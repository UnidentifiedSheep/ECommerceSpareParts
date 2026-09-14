using Abstractions.Interfaces.Exceptions;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedInternalServerException : InternalServerException, ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; }
	protected LocalizedInternalServerException(ILocalizableMessage message) : base(
		message.MessageKey)
	{
		LocalizableMessage = message;
	}

	protected LocalizedInternalServerException(
		ILocalizableMessage message,
		string details) : base(message.MessageKey, details)
	{
		LocalizableMessage = message;
	}
}
