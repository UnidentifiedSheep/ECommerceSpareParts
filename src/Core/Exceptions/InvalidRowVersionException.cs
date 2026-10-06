using Exceptions.Base;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;
using ILocalizableException = Exceptions.Interfaces.ILocalizableException;

namespace Exceptions;

public class InvalidRowVersionException() : ConflictException(null), ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; } = new LocalizableMessage("row.is.out.dated");
}
