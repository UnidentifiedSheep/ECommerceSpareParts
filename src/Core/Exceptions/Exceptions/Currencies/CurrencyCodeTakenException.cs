using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyCodeTakenException : BadRequestException
{
    [ExampleExceptionValues(false, "EXAMPLE_CURRENCY_CODE")]
    public CurrencyCodeTakenException(string code) : base("Данный код валюты занят.", new { Code = code })
    {
    }
}