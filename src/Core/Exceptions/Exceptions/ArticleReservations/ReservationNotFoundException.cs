using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleReservations;

public class ReservationNotFoundException : NotFoundException
{
    public ReservationNotFoundException(int id) : base("Не удалось найти резервацию.", new { Id = id })
    {
    }
}