using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class UnableDeserializeException(string message) : InternalServerException(message)
{
    
}