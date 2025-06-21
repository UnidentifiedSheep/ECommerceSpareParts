using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class UnableToDeleteTokenException(string? token ) : InternalServerException($"Unable to delete token {token}")
{
    
}