using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IBalanceRepository
{
    Task<Transaction?> GetTransactionByIdAsync(Guid id, bool track = true, CancellationToken ct = default);

    Task<Transaction?> GetPreviousTransactionAsync(DateTime dt, Guid userId, int currencyId, bool track = true,
        CancellationToken ct = default);

    IAsyncEnumerable<Transaction> GetAffectedTransactions(Guid userId, int currencyId, DateTime dt,
        Guid? excludeId = null, bool track = true);


    Task<UserBalance?> GetUserBalanceAsync(Guid userId, int currencyId, bool track = true,
        CancellationToken ct = default);


    Task<TransactionVersion?> GetLastTransactionVersionAsync(Guid transactionId, bool track = true,
        CancellationToken ct = default);

    Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime rangeStart, DateTime rangeEnd,
        int? currencyId, Guid? senderId, Guid? receiverId, int page, int viewCount, bool track = true,
        CancellationToken ct = default);
}