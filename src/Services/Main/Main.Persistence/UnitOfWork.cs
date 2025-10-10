using Main.Persistence.Context;
using Persistence.Services;

namespace Main.Persistence;

public class UnitOfWork(DContext context) : UnitOfWorkBase(context);