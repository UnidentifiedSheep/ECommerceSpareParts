using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Currencies;

public class CurrencyNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public CurrencyNotFoundException(object id) : base("Валюта не найдена", new { Id = id })
    {
    }

    [ExampleExceptionValues(true, 123, 456, 7890)]
    public CurrencyNotFoundException(IEnumerable<int> ids) : base("Не удалось найти валюты", new { Ids = ids })
    {
    }
}