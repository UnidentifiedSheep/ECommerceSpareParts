using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IUserRepository
{
    /// <summary>
    ///     Получение пользователя по Id.
    /// </summary>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="track">Флаг отслеживания сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null.</returns>
    Task<User?> GetUserByIdAsync(Guid userId, bool track = true, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Получение пользователя по имени пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="track">Флаг отслеживания сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null.</returns>
    Task<User?> GetUserByUserNameAsync(string userName, bool track = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Получение пользователя по email.
    /// </summary>
    /// <param name="email">Email пользователя.</param>
    /// <param name="track">Флаг отслеживания сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null.</returns>
    Task<User?> GetUserByEmailAsync(string email, bool track = true, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Получение пользователя по номеру телефона.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона пользователя.</param>
    /// <param name="track">Флаг отслеживания сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null.</returns>
    Task<User?> GetUserByPhoneAsync(string phoneNumber, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserNameTakenAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> UserExists(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetUsersBySimilarityAsync(double similarityLevel, int page, int viewCount,
        string? name = null, string? surname = null, string? email = null,
        string? phone = null, string? userName = null, Guid? id = null,
        string? description = null, bool? isSupplier = null, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetUserBySearchColumn(string? searchTerm, int page, int viewCount, bool? isSupplier = null,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<Guid>> UsersExists(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task ChangeUsersDiscount(Guid userId, decimal discount, CancellationToken cancellationToken = default);
}