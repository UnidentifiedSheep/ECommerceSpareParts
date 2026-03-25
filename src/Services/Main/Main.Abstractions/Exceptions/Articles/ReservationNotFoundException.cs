using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ReservationNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "article.reservation.not.found";
    public object[]? Arguments => null;
    public ReservationNotFoundException(int id) : base(null, new { Id = id })
    {
    }

}