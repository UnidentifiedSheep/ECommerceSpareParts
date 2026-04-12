using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Storage;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

[Transactional]
public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, Guid WhoUpdated)
    : ICommand;

public class EditArticleReservationHandler(
    IArticleReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<StorageContentReservation, int>()
        {
            Data = request.ReservationId
        }.WithTracking();
        var reservation =
            await reservationRepository.GetReservationAsync(queryOptions, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        request.NewValue.Adapt(reservation);
        reservation.WhoUpdated = request.WhoUpdated;
        reservation.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}