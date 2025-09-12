using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupNotFoundException(int id) : NotFoundException($"Не удалось найти группу наценок", new { Id = id })
{
    
}