using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.ArticleReservations;

public class InitialCountLessToCurrentException() : BadRequestException(
    "Количество которое было зарезервировано не может быть меньше текущего")
{
    
}