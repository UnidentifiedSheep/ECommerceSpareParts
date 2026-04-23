using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Storage;

namespace Main.Application.Handlers.ProductReservations.UpdateReservationsCounts;

[AutoSave]
[Transactional]
public record UpdateReservationsCountsCommand(Guid UserId, Dictionary<int, int> Contents)
    : ICommand<UpdateReservationsCountsResult>;

/// <param name="NotFoundReservations">Id артикулов и количество, которые не были найдены в резервациях у пользователя.</param>
public record UpdateReservationsCountsResult(Dictionary<int, int> NotFoundReservations);

public class UpdateReservationsCountsHandler(
    IRepository<StorageContentReservation, int> repository
    ) : ICommandHandler<UpdateReservationsCountsCommand, UpdateReservationsCountsResult>
{
    public async Task<UpdateReservationsCountsResult> Handle(
        UpdateReservationsCountsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Contents.Count == 0)
            return new UpdateReservationsCountsResult([]);
        
        var userId = request.UserId;
        var remaining = new Dictionary<int, int>(request.Contents);
        var productIds = remaining.Keys;
        
        var criteria = Criteria<StorageContentReservation>.New()
            .Where(x => x.UserId == userId && 
                        productIds.Contains(x.ProductId) && 
                        !x.IsDone)
            .Track()
            .ForUpdate()
            .Build();
        
        var reservationsByIds = (await repository.ListAsync(criteria, cancellationToken))
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.ToList());
        
        foreach (var productId in productIds)
        {
            if (!reservationsByIds.TryGetValue(productId, out var reservations))
                continue;

            foreach (var reservation in reservations)
            {
                var remainingCount = remaining[productId];
                if (remainingCount <= 0)
                    break;

                int canBeTaken = reservation.ReservedCount - reservation.CurrentCount;
                var min = Math.Min(remainingCount, canBeTaken);
                reservation.AddCount(min);
                
                remaining[productId] -= min;
            }
        }
        
        var notFoundReservations = remaining
            .Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);
        
        return new UpdateReservationsCountsResult(notFoundReservations);
    }
}