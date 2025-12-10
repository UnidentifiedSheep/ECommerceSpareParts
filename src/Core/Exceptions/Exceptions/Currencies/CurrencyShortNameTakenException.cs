using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyShortNameTakenException : BadRequestException
{
    [ExampleExceptionValues(false, "CURRENCY_SHORT_NAME")]
    public CurrencyShortNameTakenException(string shortName) : base("Короткое название валюты занято", new { ShortName = shortName })
    {
    }
}