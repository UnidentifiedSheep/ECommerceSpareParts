using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Currencies
{
	public class CurrencyNotFoundException : NotFoundException
	{
		public CurrencyNotFoundException(object id) : base("Валюта не найдена", new {Id = id})
		{
		}
		public CurrencyNotFoundException(IEnumerable<int> ids) : base($"Не удалось найти валюты", new {Ids = ids})
		{
		}
	}
}
