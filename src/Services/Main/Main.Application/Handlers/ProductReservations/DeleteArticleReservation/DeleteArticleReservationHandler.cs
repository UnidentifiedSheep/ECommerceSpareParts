using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.DeleteArticleReservation;

public record DeleteArticleReservationCommand(int ReservationId) : ICommand;

public class DeleteArticleReservationHandler(
    IArticleReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteArticleReservationCommand>
{
    public async Task<Unit> Handle(DeleteArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<StorageContentReservation, int>()
        {
            Data = request.ReservationId
        }.WithTracking();
        var reservation =
            await reservationRepository.GetReservationAsync(queryOptions, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        unitOfWork.Remove(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}