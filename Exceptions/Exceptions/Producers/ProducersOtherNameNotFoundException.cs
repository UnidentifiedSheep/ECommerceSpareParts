using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Producers;

public class ProducersOtherNameNotFoundException(string name) : NotFoundException($"Не удалось найти дополнительное имя производителя", new { Name = name })
{
    
}