using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducerNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public ProducerNotFoundException(object id) : base("Производитель не найден", new { Id = id })
    {
    }

    [ExampleExceptionValues(true, 123, 456, 7890)]
    public ProducerNotFoundException(IEnumerable<int> ids) : base("Производители не найдены", new { Ids = ids })
    {
    }
}