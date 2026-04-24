using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Transaction;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Balance;

public class TransactionRepository(DContext context) : RepositoryBase<DContext, Transaction, Guid>(context), ITransactionRepository
{
    public Task<Transaction?> GetPreviousTransactionAsync(
        DateTime dt,
        Guid userId,
        int currencyId,
        Criteria<Transaction>? criteria = null,
        CancellationToken ct = default)
    {
        var query = Context.Transactions
            .Where(x => x.TransactionDatetime <= dt)
            .Where(x => x.SenderId == userId || x.ReceiverId == userId)
            .Where(x => x.CurrencyId == currencyId)
            .OrderByDescending(x => x.TransactionDatetime)
            .ThenByDescending(x => x.Id)
            .AsQueryable();
        
        if (criteria != null)
            query = query.Apply(criteria);
            
        return query.FirstOrDefaultAsync(ct);
    }
}