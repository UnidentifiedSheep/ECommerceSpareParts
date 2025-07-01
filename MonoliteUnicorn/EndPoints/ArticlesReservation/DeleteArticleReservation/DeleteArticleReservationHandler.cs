using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.DeleteArticleReservation;

public record DeleteArticleReservationCommand(int ReservationId) : ICommand;

public class DeleteArticleReservationHandler(IArticleReservation reservationService) : ICommandHandler<DeleteArticleReservationCommand>
{
    public async Task<Unit> Handle(DeleteArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await reservationService.DeleteReservation(request.ReservationId, cancellationToken);
        return Unit.Value;
    }
}