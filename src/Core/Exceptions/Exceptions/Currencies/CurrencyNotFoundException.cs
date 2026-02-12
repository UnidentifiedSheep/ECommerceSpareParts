using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyNotFoundException : NotFoundException
{
    public CurrencyNotFoundException(object id) : base("Валюта не найдена", new { Id = id })
    {
    }
    
    public CurrencyNotFoundException(string code) : base("Валюта не найдена", new { Code = code })
    {
    }

    public CurrencyNotFoundException(IEnumerable<int> ids) : base("Не удалось найти валюты", new { Ids = ids })
    {
    }
}