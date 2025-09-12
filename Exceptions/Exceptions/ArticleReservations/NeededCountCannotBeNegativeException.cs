using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.ArticleReservations;

public class NeededCountCannotBeNegativeException() : BadRequestException("Количество в позиции не может быть отрицательным")
{
    
}