using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;
using Locan.Core.Interfaces;

namespace Exceptions;

public class InvalidInputException : BadRequestException, ILocalizableException
{
	public ILocalizableMessage LocalizableMessage { get; }
	public InvalidInputException(
		ILocalizableMessage localizableMessage,
		string? message = null) : base(message)
	{
		LocalizableMessage = localizableMessage;
	}
}
