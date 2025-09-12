using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducerNameTakenException(string producerName) : BadRequestException("Название производителя уже занято.", new { ProducerName = producerName })
{
    
}