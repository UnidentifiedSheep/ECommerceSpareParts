using Application.Common.Interfaces.Repositories;
using Main.Entities.Currency;

namespace Main.Application.Interfaces.Persistence;

public interface ICurrencyRepository : IRepository<Currency, int>
{
    
}