using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class NeededCountCannotBeNegativeException() : BadRequestException("Количество в позиции не может быть отрицательным")
{
    
}