using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyNameTakenException : BadRequestException
{
    public CurrencyNameTakenException(string name) : base("Данное имя валюты занято.", new { Name = name })
    {
    }
}