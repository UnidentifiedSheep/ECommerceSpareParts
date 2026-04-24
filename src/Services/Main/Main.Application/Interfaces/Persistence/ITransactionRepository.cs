using Application.Common.Interfaces.Repositories;
using Main.Entities.Transaction;

namespace Main.Application.Interfaces.Persistence;

public interface ITransactionRepository : IRepository<Transaction, Guid>
{
    Task<Transaction?> GetPreviousTransactionAsync(
        DateTime dt,
        Guid userId,
        int currencyId,
        Criteria<Transaction>? criteria = null,
        CancellationToken ct = default);
}