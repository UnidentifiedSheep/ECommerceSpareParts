using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product.Reservation;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductReservations.GetProductReservation;

public record GetProductReservationQuery(int ReservationId) : IQuery<GetProductReservationResult>;

public record GetProductReservationResult(ProductReservationDto Reservation);

public class GetProductReservationHandler(
	IReadRepository<ProductReservation, int> repository,
	IProjectionProvider<ProductReservation, ProductReservationDto> projection)
	: IQueryHandler<GetProductReservationQuery, GetProductReservationResult>
{
	public async Task<GetProductReservationResult> Handle(
		GetProductReservationQuery request,
		CancellationToken cancellationToken)
	{
		var reservation =
			await repository
				.Query
				.Where(x => x.Id == request.ReservationId)
				.Project(projection)
				.FirstOrDefaultAsync(cancellationToken) ??
			throw new ReservationNotFoundException(request.ReservationId);

		return new GetProductReservationResult(reservation);
	}
}
