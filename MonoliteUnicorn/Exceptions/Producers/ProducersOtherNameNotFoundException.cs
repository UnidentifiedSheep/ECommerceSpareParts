using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers;

public class ProducersOtherNameNotFoundException(string name) : NotFoundException($"Не удалось найти дополнительное имя '{name}'")
{
    
}