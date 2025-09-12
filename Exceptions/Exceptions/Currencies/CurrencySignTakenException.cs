using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencySignTakenException(string currencySign) : BadRequestException("Данный знак валюты уже занят", new { CurrencySign = currencySign })
{
    
}