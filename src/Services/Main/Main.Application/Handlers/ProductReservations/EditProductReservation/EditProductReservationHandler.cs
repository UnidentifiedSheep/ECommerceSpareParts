using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Product.Reservation;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ProductReservations.EditProductReservation;

[AutoSave]
[Transactional]
public record EditProductReservationCommand(int ReservationId, EditProductReservationDto NewValue)
    : ICommand;

public class EditProductReservationHandler(
    IRepository<StorageContentReservation, int> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<EditProductReservationCommand>
{
    public async Task<Unit> Handle(EditProductReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await repository.GetById(request.ReservationId, cancellationToken)
                          ?? throw new ReservationNotFoundException(request.ReservationId);

        var @event = ReservationManualChangeEvent.Create(reservation);
        await unitOfWork.AddAsync(@event, cancellationToken);
        
        reservation.SetComment(request.NewValue.Comment);
        reservation.ProposePrice(request.NewValue.GivenPrice, request.NewValue.GivenCurrencyId);

        return Unit.Value;
    }
}