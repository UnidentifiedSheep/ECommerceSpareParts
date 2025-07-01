using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class InitialCountMustBeGreaterThenZeroException()
    : BadRequestException("Общее количество для резервации должно быть больше 0")
{
    
}