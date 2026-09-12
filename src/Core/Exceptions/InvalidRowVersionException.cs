using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;

namespace Exceptions;

public class InvalidRowVersionException() : ConflictException(null), ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; } = new LocalizableMessage("row.is.out.dated");
}
