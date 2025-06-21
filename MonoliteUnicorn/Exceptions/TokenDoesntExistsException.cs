using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class TokenDoesntExistsException(string? token) : BadRequestException($"The token = {token}, does not exist")
{
    
}