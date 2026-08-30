using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.UnitOfWork;

public sealed class EfUnitOfWork<TContext>(TContext context) : EfUnitOfWorkBase(context)
	where TContext : DbContext;
