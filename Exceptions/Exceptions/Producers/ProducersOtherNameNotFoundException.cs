using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class ProducersOtherNameNotFoundException(string name)
    : NotFoundException("Не удалось найти дополнительное имя производителя", new { Name = name })
{
}