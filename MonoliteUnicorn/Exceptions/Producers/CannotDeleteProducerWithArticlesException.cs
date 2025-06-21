using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers;

public class CannotDeleteProducerWithArticlesException() : BadRequestException("Нельзя удалить производителя у которого есть артикулы.")
{
    
}