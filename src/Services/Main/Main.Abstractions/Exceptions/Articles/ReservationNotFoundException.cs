using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ReservationNotFoundException : NotFoundException, ILocalizableException
{
    public ReservationNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "article.reservation.not.found";
    public object[]? Arguments => null;
}