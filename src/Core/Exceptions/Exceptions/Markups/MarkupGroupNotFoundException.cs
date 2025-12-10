using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public MarkupGroupNotFoundException(int id) : base("Не удалось найти группу наценок", new { Id = id })
    {
    }
}