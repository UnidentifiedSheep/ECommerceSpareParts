using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyNameTakenException(string name) : BadRequestException("Данное имя валюты занято.", new { Name = name })
{
    
}