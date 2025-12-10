using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducerNameTakenException : BadRequestException
{
    [ExampleExceptionValues(false, "Example_Taken_Producer_Name")]
    public ProducerNameTakenException(string producerName) : base("Название производителя уже занято.", new { ProducerName = producerName })
    {
    }
}