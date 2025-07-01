using MonoliteUnicorn.Dtos.Amw.ArticleReservations;

namespace MonoliteUnicorn.Services.ArticleReservations;

public interface IArticleReservation
{
    Task CreateReservation(IEnumerable<NewArticleReservationDto> reservations, string whoCreated, CancellationToken cancellationToken = default);
    Task EditReservation(int reservationId, string whoUpdated, EditArticleReservationDto editReservationDto, CancellationToken cancellationToken = default);
    
    Task DeleteReservation(int reservationId, CancellationToken cancellationToken = default);
}