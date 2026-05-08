using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Balance;

public class TransactionRepository(DContext context)
    : RepositoryBase<DContext, Transaction, Guid>(context), ITransactionRepository
{
    public override Task<Dictionary<Guid, Transaction>> FindByIdsAsync(
        IEnumerable<Guid> ids,
        Criteria<Transaction>? criteria = null,
        CancellationToken ct = default)
    {
        return Context.Transactions
            .Where(x => ids.Contains(x.Id))
            .Apply(criteria)
            .ToDictionaryAsync(x => x.Id, x => x, ct);
    }
}