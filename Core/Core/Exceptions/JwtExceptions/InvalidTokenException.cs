namespace Core.Exceptions.JwtExceptions;

public class InvalidTokenException(string token) : BadRequestException($"This token is invalid {token}")
{
    
}