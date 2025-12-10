using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducersOtherNameNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, "Example_Producer_Other_Name")]
    public ProducersOtherNameNotFoundException(string name) : base("Не удалось найти дополнительное имя производителя", new { Name = name })
    {
    }
}