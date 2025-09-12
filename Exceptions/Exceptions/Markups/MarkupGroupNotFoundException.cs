using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Markups;

public class MarkupGroupNotFoundException(int id) : NotFoundException($"Не удалось найти группу наценок", new { Id = id })
{
    
}