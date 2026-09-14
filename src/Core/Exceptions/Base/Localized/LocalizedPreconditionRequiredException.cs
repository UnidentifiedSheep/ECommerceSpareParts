using Abstractions.Interfaces.Exceptions;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedPreconditionRequiredException : PreconditionRequiredException,
	ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; }
	protected LocalizedPreconditionRequiredException(
		ILocalizableMessage message,
		object relatedData) : base(null, relatedData)
	{
		LocalizableMessage = message;
	}
}
