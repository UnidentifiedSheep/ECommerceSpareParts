using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product.Reservation;
using Main.Entities.Storage;

namespace Main.Application.Handlers.ProductReservations.GetProductReservations;

public record GetProductReservationsQuery(
    Pagination Pagination) : IQuery<GetProductReservationsResult>;
public record GetProductReservationsResult(IReadOnlyList<ProductReservationDto> Reservations);

public class GetProductReservationsHandler(
    IReadRepository<StorageContentReservation, int> repository
    ) : IQueryHandler<GetProductReservationsQuery, GetProductReservationsResult>
{
    public async Task<GetProductReservationsResult> Handle(
        GetProductReservationsQuery request, 
        CancellationToken cancellationToken)
    {
        
    }
}