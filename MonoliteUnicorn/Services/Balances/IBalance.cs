using MonoliteUnicorn.Enums;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Balances;

public interface IBalance
{
    Task<Transaction> CreateTransactionAsync(string senderId, string receiverId, decimal amount, TransactionStatus status,
        int currencyId, string whoCreatedTransaction, DateTime transactionDateTime,
        CancellationToken cancellationToken = default);

    Task ChangeUsersDiscount(string userId, decimal discount, CancellationToken cancellationToken = default);
    Task DeleteTransaction(string transactionId, string whoDeletedUserId, CancellationToken cancellationToken = default);

    Task EditTransaction(string transactionId, int currencyId, decimal amount, TransactionStatus status,
        DateTime transactionDateTime, CancellationToken cancellationToken = default);
}