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

public class CurrencyCodeNotFoundException(string value) : LocalizedNotFoundException(
	new CurrencyNotFoundByCodeMessage().WithCode(value), new { Code = value });

public class CurrencyCodeAlreadyTakenException(string value) : LocalizedConflictException(
	new CurrencyCodeAlreadyTakeMessage().WithCode(value), new { Code = value });

public class CurrencyNameAlreadyTakenException(string value) : LocalizedConflictException(
	new CurrencyNameAlreadyTakeMessage().WithName(value), new { Name = value });

public class CurrencyShortNameAlreadyTakenException(string value) : LocalizedConflictException(
	new CurrencyShortNameAlreadyTakeMessage().WithShortName(value), new { ShortName = value });

public class CurrencySignAlreadyTakenException(string value) : LocalizedConflictException(
	new CurrencySignAlreadyTakeMessage().WithSign(value), new { Sign = value });
