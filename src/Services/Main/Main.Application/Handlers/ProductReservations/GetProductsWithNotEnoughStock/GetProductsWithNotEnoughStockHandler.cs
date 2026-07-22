using Application.Common.Interfaces.Cqrs;
using Main.Application.Interfaces.Persistence;

namespace Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;

/// <summary>
///     Получение артикулов у которых не хватает количества на складе,
///     или есть резервации артикулов другими пользователями при этом количество на складе
///     не покрывает продажу и резервацию.
/// </summary>
public record GetProductsWithNotEnoughStockQuery(
    Guid BuyerOrganizationId,
    string StorageName,
    bool TakeFromOtherStorages,
    Dictionary<int, int> NeededCounts
) : IQuery<GetProductsWithNotEnoughStockResult>;

/// <param name="NotEnoughByReservation">Список артикулов которых не хватает из-за резерваций</param>
/// <param name="NotEnoughByStock">Список артикулов которых не хватает на складе для продажи без учета резерваций.</param>
public record GetProductsWithNotEnoughStockResult(
    Dictionary<int, int> NotEnoughByReservation,
    Dictionary<int, int> NotEnoughByStock
);

public class GetProductsWithNotEnoughStockHandler(
    IStorageContentRepository storageContentRepository,
    IProductReservationRepository reservationRepository
)
    : IQueryHandler<GetProductsWithNotEnoughStockQuery, GetProductsWithNotEnoughStockResult>
{
    public async Task<GetProductsWithNotEnoughStockResult> Handle(
        GetProductsWithNotEnoughStockQuery request,
        CancellationToken cancellationToken)
    {
        var articleIds = request.NeededCounts.Keys;
        var storageName = request.StorageName;
        var organizationId = request.BuyerOrganizationId;
        var takeFromOtherStorages = request.TakeFromOtherStorages;

        var notEnoughByReservation = new Dictionary<int, int>();
        var notEnoughStock = new Dictionary<int, int>();

        var organizationReservations =
            await reservationRepository.GetReservationsCountForOrganizationAsync(
                organizationId,
                articleIds,
                cancellationToken);
        var otherOrganizationsReservations =
            await reservationRepository.GetOtherOrganizationsReservationsCountAsync(
                organizationId,
                articleIds,
                cancellationToken);
        var storageCounts = await storageContentRepository.GetStorageContentCounts(
            storageName,
            articleIds,
            takeFromOtherStorages,
            cancellationToken);

        foreach (var (id, count) in request.NeededCounts)
        {
            storageCounts.TryGetValue(id, out var storageCount);
            otherOrganizationsReservations.TryGetValue(id, out var reservationsCount);
            organizationReservations.TryGetValue(id, out var organizationReservation);

            var stockDiff = storageCount - count;
            if (stockDiff < 0)
            {
                notEnoughStock.Add(id, Math.Abs(stockDiff));
                continue;
            }

            var reservationsDiff = stockDiff - reservationsCount + organizationReservation;
            if (reservationsDiff < 0) notEnoughByReservation.Add(id, Math.Abs(reservationsDiff));
        }

        return new GetProductsWithNotEnoughStockResult(notEnoughByReservation, notEnoughStock);
    }
}
