using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.UnitOfWork;

public sealed class UnitOfWork<TContext>(TContext context) : UnitOfWorkBase(context)
    where TContext : DbContext;