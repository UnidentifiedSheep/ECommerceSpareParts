using Analytics.Persistence.Context;
using Persistence.Services;

namespace Analytics.Persistence;

public class UnitOfWork(DContext context) : UnitOfWorkBase(context);