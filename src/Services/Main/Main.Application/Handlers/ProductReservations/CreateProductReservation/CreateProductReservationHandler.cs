using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Projections;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

[Transactional]
public record CreateProductReservationCommand(
    NewProductReservationDto Reservation
) : ICommand<CreateProductReservationResult>;

public record CreateProductReservationResult(ProductReservationDto Reservation);

public class CreateProductReservationHandler(
    IUnitOfWork unitOfWork,
    IReadRepository<StorageContentReservation, int> repository
) : ICommandHandler<CreateProductReservationCommand, CreateProductReservationResult>
{
    public async Task<CreateProductReservationResult> Handle(
        CreateProductReservationCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Reservation;
        var reservation = StorageContentReservation.Create(
            dto.UserId,
            dto.ProductId,
            dto.ReservedCount);

        reservation.SetComment(dto.Comment);
        reservation.ProposePrice(dto.ProposedPrice, dto.GivenCurrencyId);
        reservation.AddCount(dto.CurrentCount);

        await unitOfWork.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await repository.Query
            .Where(x => x.Id == reservation.Id)
            .AsExpandable()
            .Select(ProductProjections.ToReservationDto)
            .SingleAsync(cancellationToken);

        return new CreateProductReservationResult(result);
    }
}