using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Producers;

public class ProducerNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "producer.not.found";
    public object[]? Arguments => null;
    public ProducerNotFoundException(int id) : base(null, new { Id = id })
    {
    }

}