using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class CannotDeleteProducerWithArticlesException : BadRequestException
{
    [ExampleExceptionValues]
    public CannotDeleteProducerWithArticlesException() : base("Нельзя удалить производителя у которого есть артикулы.")
    {
    }
}