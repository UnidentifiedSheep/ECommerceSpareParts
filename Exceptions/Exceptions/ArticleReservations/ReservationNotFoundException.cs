using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.ArticleReservations;

public class ReservationNotFoundException(int id) : NotFoundException("Не удалось найти резервацию.", new {Id = id})
{
    
}