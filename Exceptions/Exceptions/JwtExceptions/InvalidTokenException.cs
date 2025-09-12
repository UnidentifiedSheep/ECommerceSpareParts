using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.JwtExceptions;

public class InvalidTokenException(string token) : BadRequestException($"This token is invalid {token}")
{
    
}