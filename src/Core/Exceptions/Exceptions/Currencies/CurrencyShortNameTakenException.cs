using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyShortNameTakenException : BadRequestException
{
    public CurrencyShortNameTakenException(string shortName) : base("Короткое название валюты занято", new { ShortName = shortName })
    {
    }
}