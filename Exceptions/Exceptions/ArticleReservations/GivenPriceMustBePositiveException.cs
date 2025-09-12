using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.ArticleReservations;

public class GivenPriceMustBePositiveException() : BadRequestException("Цена не может быть отрицательной или равной 0")
{
    
}