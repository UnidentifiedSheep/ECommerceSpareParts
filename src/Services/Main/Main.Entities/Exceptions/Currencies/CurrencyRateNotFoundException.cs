using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Currencies;

public class CurrencyRateNotFoundException : NotFoundException, ILocalizableException
{
    public CurrencyRateNotFoundException(int currencyId) : base(null, new { CurrencyId = currencyId })
    {
    }

    public string MessageKey => "currency.rate.not.found";
    public object[]? Arguments => null;
}