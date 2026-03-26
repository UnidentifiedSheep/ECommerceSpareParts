using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Currencies;

public class CurrencyNotFoundException : NotFoundException, ILocalizableException
{
    public CurrencyNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "currency.not.found";
    public object[]? Arguments => null;
}