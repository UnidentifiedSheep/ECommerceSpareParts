using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Producers;

public class ProducersOtherNameNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "producer.additional.name.not.found";
    public object[]? Arguments { get; }
    public ProducersOtherNameNotFoundException(string name) : base(null, new { Name = name })
    {
        Arguments = [name];
    }

}