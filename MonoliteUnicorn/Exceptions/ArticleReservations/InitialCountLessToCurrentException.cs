using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class InitialCountLessToCurrentException() : BadRequestException(
    "Количество которое было зарезервировано не может быть меньше текущего")
{
    
}