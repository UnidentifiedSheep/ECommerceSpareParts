using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class InvalidResponseException(string message) : InternalServerException(message)
{
    
}