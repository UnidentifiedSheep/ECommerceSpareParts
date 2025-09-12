using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IBalanceRepository
{
    Task<Transaction?> GetTransactionByIdAsync(string id, bool track = true, CancellationToken ct = default);

    Task<bool> TransactionExistsAsync(string senderId, string receiverId, DateTime dt, string? exceptId = null,
        CancellationToken ct = default);

    Task<Transaction?> GetPreviousTransactionAsync(DateTime dt, string userId, int currencyId, bool track = true,
        CancellationToken ct = default);

    IAsyncEnumerable<Transaction> GetAffectedTransactions(string userId, int currencyId, DateTime dt,
        string? excludeId = null, bool track = true);
    

    Task<UserBalance?> GetUserBalanceAsync(string userId, int currencyId, bool track = true,
        CancellationToken ct = default);
    

    Task<TransactionVersion?> GetLastTransactionVersionAsync(string transactionId, bool track = true,
        CancellationToken ct = default);

    Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime rangeStart, DateTime rangeEnd,
        int? currencyId, string? senderId, string? receiverId, int page, int viewCount, bool track = true, CancellationToken ct = default);
    
    Task<bool> TransactionExistsAsync(string transactionId, CancellationToken ct = default);
}