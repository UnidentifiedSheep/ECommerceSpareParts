using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Product.Reservation;
using Main.Entities.Storage;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

[Transactional]
public record CreateProductReservationCommand(NewProductReservationDto Reservation)
	: ICommand<CreateProductReservationResult>;

public record CreateProductReservationResult(int ReservationId);

public class CreateProductReservationHandler(IUnitOfWork unitOfWork)
	: ICommandHandler<CreateProductReservationCommand, CreateProductReservationResult>
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

		return new CreateProductReservationResult(reservation.Id);
	}
}
