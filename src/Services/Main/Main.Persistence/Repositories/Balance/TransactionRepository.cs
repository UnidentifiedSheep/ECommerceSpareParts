using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Balance;

public class TransactionRepository(DContext context) : RepositoryBase<DContext, Transaction, Guid>(context), ITransactionRepository
{
}