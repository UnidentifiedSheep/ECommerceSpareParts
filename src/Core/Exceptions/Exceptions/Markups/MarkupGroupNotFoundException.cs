using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupNotFoundException : NotFoundException
{
    public MarkupGroupNotFoundException(int id) : base("Не удалось найти группу наценок", new { Id = id })
    {
    }
}