using Abstractions.Interfaces.Exceptions;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedNotFoundException : NotFoundException, ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; }
	protected LocalizedNotFoundException(ILocalizableMessage message) : base(null)
	{
		LocalizableMessage = message;
	}

	protected LocalizedNotFoundException(
		ILocalizableMessage message,
		object relatedData) : base(null, relatedData)
	{
		LocalizableMessage = message;
	}
}
