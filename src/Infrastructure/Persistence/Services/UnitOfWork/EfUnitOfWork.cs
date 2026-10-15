using Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.UnitOfWork;

public sealed class EfUnitOfWork<TContext>(
	TContext context,
	ITransactionExceptionClassifier exceptionClassifier) : EfUnitOfWorkBase(context, exceptionClassifier)
	where TContext : DbContext;
