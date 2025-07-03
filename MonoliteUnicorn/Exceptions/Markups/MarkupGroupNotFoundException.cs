using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Markups;

public class MarkupGroupNotFoundException(int id) : NotFoundException($"Не удалось найти группу наценок с id: {id}")
{
    
}