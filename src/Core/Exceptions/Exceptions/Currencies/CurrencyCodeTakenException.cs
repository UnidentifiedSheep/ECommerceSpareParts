using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyCodeTakenException : BadRequestException
{
    public CurrencyCodeTakenException(string code) : base("Данный код валюты занят.", new { Code = code })
    {
    }
}