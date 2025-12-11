using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducersOtherNameNotFoundException : NotFoundException
{
    public ProducersOtherNameNotFoundException(string name) : base("Не удалось найти дополнительное имя производителя", new { Name = name })
    {
    }
}