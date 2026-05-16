using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Currency;

public class CurrencyRepository(DContext context)
    : LinqRepositoryBase<DContext, Entities.Currency.Currency, int>(context), ICurrencyRepository;
