using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ProductReservations.DeleteProductReservation;

[AutoSave]
[Transactional]
public record DeleteProductReservationCommand(int ReservationId) : ICommand;

public class DeleteProductReservationHandler(
    IRepository<StorageContentReservation, int> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductReservationCommand>
{
    public async Task<Unit> Handle(DeleteProductReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await repository.GetById(request.ReservationId, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        unitOfWork.Remove(reservation);
        return Unit.Value;
    }
}