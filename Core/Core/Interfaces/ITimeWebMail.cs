using Core.Dtos.TimeWebCloud.Responses;

namespace Core.Interfaces;

public interface ITimeWebMail
{
    /// <summary>
    ///     Получение списка почтовых ящиков аккаунта
    /// </summary>
    /// <param name="limit">Обозначает количество записей, которое необходимо вернуть.</param>
    /// <param name="offset">Указывает на смещение относительно начала списка.</param>
    /// <param name="search">Поиск почтового ящика по названию</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetMailsResponse> GetMails(int limit = 100, int offset = 0, string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// </summary>
    /// <param name="domain">Полное имя домена</param>
    /// <param name="limit">Обозначает количество записей, которое необходимо вернуть.</param>
    /// <param name="offset">Указывает на смещение относительно начала списка.</param>
    /// <param name="search">Поиск почтового ящика по названию</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetMailsOfDomainResponse> GetMailsOfDomain(string domain, int limit = 100, int offset = 0,
        string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Создание почты
    /// </summary>
    /// <param name="domain">Имя домена</param>
    /// <param name="mailBox">Почтовый адрес</param>
    /// <param name="password">Пароль</param>
    /// <param name="comment">Комментарий</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateMail(string domain, string mailBox, string password, string comment = "",
        CancellationToken cancellationToken = default);
}