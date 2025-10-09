using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyShortNameTakenException(string shortName)
    : BadRequestException("Короткое название валюты занято", new { ShortName = shortName })
{
}