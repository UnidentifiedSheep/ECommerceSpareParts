using Locan.Core.Interfaces;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions.Base.Localized;

public abstract class LocalizedPreconditionRequiredException : PreconditionRequiredException,
	ILocalizableException
{
	protected LocalizedPreconditionRequiredException(ILocalizableMessage message, object relatedData) : base(
		null,
		relatedData)
	{
		LocalizableMessage = message;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
