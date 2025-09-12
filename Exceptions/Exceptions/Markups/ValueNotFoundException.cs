using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Markups;

public class ValueNotFoundException(string where, string what) : NotFoundException($"Value '{what}', not found in '{where}'")
{
    
}