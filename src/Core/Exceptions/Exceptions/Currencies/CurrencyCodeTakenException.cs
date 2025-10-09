using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyCodeTakenException(string code)
    : BadRequestException("Данный код валюты занят.", new { Code = code })
{
}