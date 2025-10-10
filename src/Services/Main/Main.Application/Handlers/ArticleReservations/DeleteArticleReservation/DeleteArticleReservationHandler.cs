using Application.Common.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleReservations;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.DeleteArticleReservation;

public record DeleteArticleReservationCommand(int ReservationId) : ICommand;

public class DeleteArticleReservationHandler(
    IArticleReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteArticleReservationCommand>
{
    public async Task<Unit> Handle(DeleteArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation =
            await reservationRepository.GetReservationAsync(request.ReservationId, true, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        unitOfWork.Remove(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}