using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducerNameTakenException : BadRequestException
{
    public ProducerNameTakenException(string producerName) : base("Название производителя уже занято.", new { ProducerName = producerName })
    {
    }
}