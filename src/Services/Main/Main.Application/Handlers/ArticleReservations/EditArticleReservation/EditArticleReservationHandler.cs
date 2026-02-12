using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.ArticleReservations;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

[Transactional]
public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, Guid WhoUpdated)
    : ICommand;

public class EditArticleReservationHandler(IArticleReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation =
            await reservationRepository.GetReservationAsync(request.ReservationId, true, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        request.NewValue.Adapt(reservation);
        reservation.WhoUpdated = request.WhoUpdated;
        reservation.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}