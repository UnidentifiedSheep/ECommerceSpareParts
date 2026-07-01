using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Balance;

public class TransactionRepository(DContext context, IQueryableExtensions extensions)
    : LinqRepositoryBase<DContext, Transaction, Guid>(context, extensions), ITransactionRepository;