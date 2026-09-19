using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;
using Locan.Core.Interfaces;

namespace Exceptions;

public class InvalidInputException : BadRequestException, ILocalizableException
{
	public InvalidInputException(ILocalizableMessage localizableMessage, string? message = null) : base(
		message)
	{
		LocalizableMessage = localizableMessage;
	}
	public ILocalizableMessage LocalizableMessage { get; }
}
