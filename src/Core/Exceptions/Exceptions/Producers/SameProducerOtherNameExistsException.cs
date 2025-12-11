using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class SameProducerOtherNameExistsException : BadRequestException
{
    public SameProducerOtherNameExistsException() : base("Дополнительное название производителя, с таким использованием уже есть.")
    {
    }
}