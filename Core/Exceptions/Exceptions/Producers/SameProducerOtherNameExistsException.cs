using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class SameProducerOtherNameExistsException()
    : BadRequestException("Дополнительное название производителя, с таким использованием уже есть.")
{
}