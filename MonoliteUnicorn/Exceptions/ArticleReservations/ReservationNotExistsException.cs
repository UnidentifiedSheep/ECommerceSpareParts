using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.ArticleReservations;

public class ReservationNotExistsException(int id) : NotFoundException("Не удалось найти резервацию.", new {Id = id})
{
    
}