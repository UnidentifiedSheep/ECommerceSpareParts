using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Producers;

public class ProducerNotFoundException : NotFoundException, ILocalizableException
{
    public ProducerNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "producer.not.found";
    public object[]? Arguments => null;
}