using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleReservations;

public class ReservationNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public ReservationNotFoundException(int id) : base("Не удалось найти резервацию.", new { Id = id })
    {
    }
}