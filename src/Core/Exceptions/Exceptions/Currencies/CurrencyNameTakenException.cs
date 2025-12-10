using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyNameTakenException : BadRequestException
{
    [ExampleExceptionValues(false, "EXAMPLE_CURRENCY_NAME")]
    public CurrencyNameTakenException(string name) : base("Данное имя валюты занято.", new { Name = name })
    {
    }
}