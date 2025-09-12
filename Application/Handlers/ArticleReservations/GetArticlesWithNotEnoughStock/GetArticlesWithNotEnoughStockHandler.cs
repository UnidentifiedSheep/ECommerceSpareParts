using Application.Extensions;
using Application.Interfaces;
using Core.Interfaces.DbRepositories;

namespace Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;

/// <summary>
/// Получение артикулов у которых не хватает количества на складе,
/// или есть резервации артикулов другими пользователями при этом количество на складе
/// не покрывает продажу и резервацию.
/// </summary>
public record GetArticlesWithNotEnoughStockQuery(string BuyerId, string StorageName, bool TakeFromOtherStorages, 
    Dictionary<int, int> NeededCounts) : IQuery<GetArticlesWithNotEnoughStockResult>;

/// <param name="NotEnoughByReservation">Список артикулов которых не хватает из-за резерваций</param>
/// <param name="NotEnoughByStock">Список артикулов которых не хватает на складе для продажи без учета резерваций.</param>
public record GetArticlesWithNotEnoughStockResult(Dictionary<int, int> NotEnoughByReservation, Dictionary<int, int> NotEnoughByStock);

public class GetArticlesWithNotEnoughStockHandler(IUsersRepository usersRepository, IArticlesRepository articlesRepository,
    IStoragesRepository storagesRepository, IStorageContentRepository storageContentRepository, 
    IArticleReservationRepository reservationRepository) : IQueryHandler<GetArticlesWithNotEnoughStockQuery, GetArticlesWithNotEnoughStockResult>
{
    public async Task<GetArticlesWithNotEnoughStockResult> Handle(GetArticlesWithNotEnoughStockQuery request, CancellationToken cancellationToken)
    {
        var articleIds = request.NeededCounts.Keys;
        var storageName = request.StorageName;
        var userId = request.BuyerId;
        var takeFromOtherStorages = request.TakeFromOtherStorages;
        await EnsureDataExists(storageName, userId, articleIds, cancellationToken);
        
        var notEnoughByReservation = new Dictionary<int, int>();
        var notEnoughStock = new Dictionary<int, int>();
        
        var userReservations = await reservationRepository.GetReservationsCountForUserAsync(userId, articleIds, cancellationToken); 
        var othersReservations = await reservationRepository.GetReservationsCountForOthersAsync(userId, articleIds, cancellationToken);
        var storageCounts = await storageContentRepository.GetStorageContentCounts(storageName, articleIds, takeFromOtherStorages, cancellationToken);
        
        foreach (var (id, count) in request.NeededCounts)
        {
            storageCounts.TryGetValue(id, out var storageCount);
            othersReservations.TryGetValue(id, out var reservationsCount);
            userReservations.TryGetValue(id, out var userReservation);
            
            var stockDiff = storageCount - count;
            if (stockDiff < 0)
            {
                notEnoughStock.Add(id, Math.Abs(stockDiff));
                continue;
            }
            var reservationsDiff = stockDiff - reservationsCount + userReservation;
            if(reservationsDiff < 0) notEnoughByReservation.Add(id, Math.Abs(reservationsDiff));
        }
        
        return new GetArticlesWithNotEnoughStockResult(notEnoughByReservation, notEnoughStock);
    }

    private async Task EnsureDataExists(string storageName, string userId, IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        await storagesRepository.EnsureStorageExists(storageName, cancellationToken);
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
        await articlesRepository.EnsureArticlesExist(articleIds, cancellationToken);
    }
}