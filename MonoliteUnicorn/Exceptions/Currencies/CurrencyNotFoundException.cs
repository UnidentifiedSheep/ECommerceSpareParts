using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Currencies
{
	public class CurrencyNotFoundException : NotFoundException
	{
		public CurrencyNotFoundException(object key) : base("Валюта не найдена", key)
		{
		}
		public CurrencyNotFoundException(IEnumerable<int> ids) : base($"Не удалось найти валюты ({string.Join(',', ids)})")
		{
		}
	}
}
