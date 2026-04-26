using Application.Common.Interfaces.Repositories;
using Main.Entities.Balance;

namespace Main.Application.Interfaces.Persistence;

public interface ITransactionRepository : IRepository<Transaction, Guid>
{
}