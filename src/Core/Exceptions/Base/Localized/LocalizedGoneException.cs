using Locan.Core.Interfaces;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions.Base.Localized;

public abstract class LocalizedGoneException : GoneException, ILocalizableException
{
	protected LocalizedGoneException(ILocalizableMessage message) : base(null)
	{
		LocalizableMessage = message;
	}

	protected LocalizedGoneException(ILocalizableMessage message, object relatedData)
		: base(null, relatedData)
	{
		LocalizableMessage = message;
	}

	public ILocalizableMessage LocalizableMessage { get; }
}
