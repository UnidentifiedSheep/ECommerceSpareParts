using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencySignTakenException : BadRequestException
{
    public CurrencySignTakenException(string currencySign) : base("Данный знак валюты уже занят", new { CurrencySign = currencySign })
    {
    }
}