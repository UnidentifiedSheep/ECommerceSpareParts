using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Product.Reservation;
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
    IReadRepository<ProductReservation, int> repository,
    IProjectionProvider<ProductReservation, ProductReservationDto> projection
) : ICommandHandler<CreateProductReservationCommand, CreateProductReservationResult>
{
    public async Task<CreateProductReservationResult> Handle(
        CreateProductReservationCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Reservation;
        var reservation = ProductReservation.Create(
            dto.OrganizationId,
            dto.ProductId,
            dto.ReservedCount);

        reservation.SetComment(dto.Comment);
        reservation.ProposePrice(dto.ProposedPrice, dto.GivenCurrencyId);
        reservation.AddCount(dto.CurrentCount);

        await unitOfWork.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await repository.Query
            .Where(x => x.Id == reservation.Id)
            .Project(projection)
            .SingleAsync(cancellationToken);

        return new CreateProductReservationResult(result);
    }
}
