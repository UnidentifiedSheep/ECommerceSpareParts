using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class ValueNotFoundException(string where, string what) : NotFoundException($"Value '{what}', not found in '{where}'")
{
    
}