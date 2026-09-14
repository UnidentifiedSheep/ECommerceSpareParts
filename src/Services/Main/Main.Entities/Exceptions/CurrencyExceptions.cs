using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CurrencyNotFoundException : LocalizedNotFoundException
{
	public CurrencyNotFoundException(int id) : base(
		CurrencyNotFoundMessage.Instance,
		new
		{
			Id = id
		})
	{
	}

	public CurrencyNotFoundException(IEnumerable<int> ids) : base(
		CurrencyNotFoundMessage.Instance,
		new
		{
			Ids = ids
		})
	{
	}
}

public class CurrencyRateNotFoundException(int currencyId) : LocalizedNotFoundException(
	CurrencyRateNotFoundMessage.Instance,
	new
	{
		CurrencyId = currencyId
	});
