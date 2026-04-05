using Abstractions.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.UnitOfWork;

public sealed class UnitOfWork<TContext>(TContext context, IUnitOfWorkContext uowContext) 
    : UnitOfWorkBase(context, uowContext) where TContext : DbContext;