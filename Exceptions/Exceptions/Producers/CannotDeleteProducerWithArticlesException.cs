using Exceptions.Base;

namespace Exceptions.Exceptions.Producers;

public class CannotDeleteProducerWithArticlesException() : BadRequestException("Нельзя удалить производителя у которого есть артикулы.")
{
    
}