using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Event;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductReservations.GetReservationHistory;

public record GetReservationHistoryQuery(
    int ReservationId,
    Pagination Pagination) : IQuery<GetReservationHistoryResult>;

public record GetReservationHistoryResult(IReadOnlyList<ReservationManualChangeEventData> History);

public class GetReservationHistoryHandler(
    IReadRepository<Event, int> repository) : IQueryHandler<GetReservationHistoryQuery, GetReservationHistoryResult>
{
    public async Task<GetReservationHistoryResult> Handle(
        GetReservationHistoryQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await repository
            .Query
            .OfType<ReservationManualChangeEvent>()
            .Where(x => x.ReservationId == request.ReservationId)
            .OrderByDescending(x => x.Id)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetReservationHistoryResult(result.Select(x => x.Data).ToList());
    }
}