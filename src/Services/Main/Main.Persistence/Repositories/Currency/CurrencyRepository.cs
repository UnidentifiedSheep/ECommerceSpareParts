using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Currency;

public class CurrencyRepository(DContext context, IQueryableExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.Currency.Currency, int>(context, extensions), ICurrencyRepository;