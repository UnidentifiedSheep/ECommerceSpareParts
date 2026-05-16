using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Persistence.Context;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Balance;

public class TransactionRepository(DContext context)
    : LinqRepositoryBase<DContext, Transaction, Guid>(context), ITransactionRepository;
