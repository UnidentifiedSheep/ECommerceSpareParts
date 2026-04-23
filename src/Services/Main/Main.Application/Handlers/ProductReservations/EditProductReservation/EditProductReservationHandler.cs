using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Dtos.Product;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ProductReservations.EditProductReservation;

[AutoSave]
[Transactional]
public record EditProductReservationCommand(int ReservationId, EditProductReservationDto NewValue)
    : ICommand;

public class EditProductReservationHandler(
    IRepository<StorageContentReservation, int> repository
    ) : ICommandHandler<EditProductReservationCommand>
{
    public async Task<Unit> Handle(EditProductReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await repository.GetById(request.ReservationId, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        
        reservation.SetComment(request.NewValue.Comment);
        reservation.ProposePrice(request.NewValue.GivenPrice, request.NewValue.GivenCurrencyId);
        
        return Unit.Value;
    }
}