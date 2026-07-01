using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ProductReservations.DeleteProductReservation;

[AutoSave]
[Transactional]
public record DeleteProductReservationCommand(int ReservationId) : ICommand;

public class DeleteProductReservationHandler(
    IRepository<StorageContentReservation, int> repository
) : ICommandHandler<DeleteProductReservationCommand>
{
    public async Task<Unit> Handle(
        DeleteProductReservationCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await repository.GetById(request.ReservationId, cancellationToken)
                          ?? throw new ReservationNotFoundException(request.ReservationId);
        reservation.Cancel();
        return Unit.Value;
    }
}