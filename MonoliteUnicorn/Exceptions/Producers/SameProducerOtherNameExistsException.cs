using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers;

public class SameProducerOtherNameExistsException() : BadRequestException("Дополнительное название производителя, с таким использованием уже есть.")
{
    
}