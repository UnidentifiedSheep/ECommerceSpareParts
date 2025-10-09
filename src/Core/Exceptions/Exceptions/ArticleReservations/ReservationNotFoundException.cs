using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleReservations;

public class ReservationNotFoundException(int id) : NotFoundException("Не удалось найти резервацию.", new { Id = id })
{
}