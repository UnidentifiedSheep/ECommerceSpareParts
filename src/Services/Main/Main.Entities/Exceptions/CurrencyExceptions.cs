using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CurrencyNotFoundException : LocalizedNotFoundException
{
    public CurrencyNotFoundException(int id)
        : base("currency.not.found", new { Id = id })
    {
    }

    public CurrencyNotFoundException(IEnumerable<int> ids)
        : base("currency.not.found", new { Ids = ids })
    {
    }
}

public class CurrencyRateNotFoundException(int currencyId)
    : LocalizedNotFoundException("currency.rate.not.found", new { CurrencyId = currencyId });
