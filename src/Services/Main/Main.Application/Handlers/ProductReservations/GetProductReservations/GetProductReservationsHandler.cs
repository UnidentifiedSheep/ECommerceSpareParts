using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Projections;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductReservations.GetProductReservations;

public record GetProductReservationsQuery(
    int? ProductId,
    Guid? UserId,
    string? SortBy,
    bool ShowDeleted,
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
        var result = await repository
            .Query
            .Where(x => !request.ProductId.HasValue || x.ProductId == request.ProductId.Value)
            .Where(x => !request.UserId.HasValue || x.UserId == request.UserId.Value)
            .Where(x => request.ShowDeleted || x.Status != StorageContentReservationStatus.Canceled)
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(ProductProjections.ToReservationDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetProductReservationsResult(result);
    }
}
