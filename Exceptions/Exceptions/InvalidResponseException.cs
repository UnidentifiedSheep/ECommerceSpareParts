using Exceptions.Base;

namespace Exceptions.Exceptions;

public class InvalidResponseException(string message) : InternalServerException(message)
{
}