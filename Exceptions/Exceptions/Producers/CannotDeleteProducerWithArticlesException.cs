using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Producers;

public class CannotDeleteProducerWithArticlesException() : BadRequestException("Нельзя удалить производителя у которого есть артикулы.")
{
    
}