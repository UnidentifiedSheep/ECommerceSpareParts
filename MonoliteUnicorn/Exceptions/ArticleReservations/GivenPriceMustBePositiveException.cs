using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class GivenPriceMustBePositiveException() : BadRequestException("Цена не может быть отрицательной или равной 0")
{
    
}