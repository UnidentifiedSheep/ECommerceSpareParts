using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Currency;

public class CurrencyRate : AuditableEntity<CurrencyRate, int>, ILinqEntity<CurrencyRate, int>
{
	private CurrencyRate() { }

	private CurrencyRate(int fromCurrencyId, int toCurrencyId, decimal rate)
	{
		if (fromCurrencyId == toCurrencyId)
			throw new InvalidOperationException("Currency rate self reference is not permitted.");
		if (rate <= 0m)
			throw new InvalidOperationException("Currency rate must be greater than 0.");
		FromCurrencyId = fromCurrencyId;
		ToCurrencyId = toCurrencyId;
		Rate = rate;
	}

	public int Id { get; private set; }
	public int FromCurrencyId { get; private set; }
	public int ToCurrencyId { get; private set; }
	public decimal Rate { get; private set; }
	public Currency FromCurrency { get; private set; } = null!;
	public Currency ToCurrency { get; private set; } = null!;

	public static CurrencyRate Create(int fromCurrencyId, int toCurrencyId, decimal rate) =>
		new(fromCurrencyId, toCurrencyId, rate);

	public static Expression<Func<CurrencyRate, int>> GetKeySelector() => value => value.Id;
	public static Expression<Func<CurrencyRate, bool>> GetEqualityExpression(int key) => value => value.Id == key;
	public override int GetId() => Id;
}
