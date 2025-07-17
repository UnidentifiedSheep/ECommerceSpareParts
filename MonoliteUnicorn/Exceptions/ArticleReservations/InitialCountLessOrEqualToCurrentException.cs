using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class InitialCountLessOrEqualToCurrentException() : BadRequestException(
    "Количество которое было зарезервировано не может быть меньше текущего, при создании резервации")
{
    
}