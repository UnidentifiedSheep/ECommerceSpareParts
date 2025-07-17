using MonoliteUnicorn.Dtos.Amw.ArticleReservations;

namespace MonoliteUnicorn.Services.ArticleReservations;

public interface IArticleReservation
{
    Task CreateReservation(IEnumerable<NewArticleReservationDto> reservations, string whoCreated, CancellationToken cancellationToken = default);
    Task EditReservation(int reservationId, string whoUpdated, EditArticleReservationDto editReservationDto, CancellationToken cancellationToken = default);
    Task DeleteReservation(int reservationId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Получение артикулов у которых не хватает количества на складе,
    /// или есть резервации артикулов другими пользователями при этом количество на складе
    /// не покрывает продажу и резервацию.. 
    /// </summary>
    /// <param name="userId">Id Пользователя который покупает</param>
    /// <param name="storageName">Название склада</param>
    /// <param name="takeFromOtherStorages">Брать ли с других складов</param>
    /// <param name="neededCounts">Нужные количества ArticleId, Count</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Список артикулов которых не хватает из-за резерваций,
    /// список артикулов которых не хватает на складе для продажи без учета резерваций.</returns>
    Task<(Dictionary<int, int>, Dictionary<int, int>)> GetArticlesWithNotEnoughStock(string userId, string storageName, bool takeFromOtherStorages,
        Dictionary<int, int> neededCounts, CancellationToken cancellationToken = default);
}