using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Producers;

public class SameProducerOtherNameExistsException() : BadRequestException("Дополнительное название производителя, с таким использованием уже есть.")
{
    
}