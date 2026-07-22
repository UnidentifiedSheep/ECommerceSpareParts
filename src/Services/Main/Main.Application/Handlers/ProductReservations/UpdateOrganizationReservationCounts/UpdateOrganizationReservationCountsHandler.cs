using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Application.Handlers.ProductReservations.UpdateOrganizationReservationCounts;

[AutoSave]
[Transactional]
public record UpdateOrganizationReservationCountsCommand(
    Guid OrganizationId,
    Dictionary<int, int> Contents)
    : ICommand<UpdateOrganizationReservationCountsResult>;

/// <param name="NotFoundReservations">Id продуктов и количество, не найденное в резервациях организации.</param>
public record UpdateOrganizationReservationCountsResult(
    Dictionary<int, int> NotFoundReservations);

public class UpdateOrganizationReservationCountsHandler(
    IRepository<ProductReservation, int> repository
) : ICommandHandler<UpdateOrganizationReservationCountsCommand, UpdateOrganizationReservationCountsResult>
{
    public async Task<UpdateOrganizationReservationCountsResult> Handle(
        UpdateOrganizationReservationCountsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Contents.Count == 0) return new UpdateOrganizationReservationCountsResult([]);

        var organizationId = request.OrganizationId;
        var remaining = new Dictionary<int, int>(request.Contents);
        var productIds = remaining.Keys;

        var criteria = Criteria<ProductReservation>.New()
            .Where(x => x.OrganizationId == organizationId &&
                        productIds.Contains(x.ProductId) &&
                        (x.Status == ProductReservationStatus.Active ||
                         x.Status == ProductReservationStatus.Locked))
            .Track()
            .ForUpdate()
            .Build();

        var reservationsByIds = (await repository.ListAsync(criteria, cancellationToken))
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var productId in productIds)
        {
            if (!reservationsByIds.TryGetValue(productId, out var reservations)) continue;

            foreach (var reservation in reservations)
            {
                var remainingCount = remaining[productId];
                if (remainingCount <= 0) break;

                var canBeTaken = reservation.ReservedCount - reservation.CurrentCount;
                var min = Math.Min(remainingCount, canBeTaken);
                reservation.AddCount(min);

                remaining[productId] -= min;
            }
        }

        var notFoundReservations = remaining
            .Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);

        return new UpdateOrganizationReservationCountsResult(notFoundReservations);
    }
}
