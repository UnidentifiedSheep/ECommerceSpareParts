using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Currency;

public class Currency : Entity<Currency, int>, ILinqEntity<Currency, int>
{
	private readonly List<CurrencyRate> _ratesFrom = [];

	private readonly List<CurrencyRate> _ratesTo = [];

	private Currency()
	{
	}

	private Currency(
		string name,
		string shortName,
		string currencySign,
		string code)
	{
		SetName(name);
		SetShortName(shortName);
		SetCurrencySign(currencySign);
		SetCode(code);
	}

	[Validate]
	public int Id { get; private set; }

	[Validate]
	public string ShortName { get; private set; } = null!;

	[Validate]
	public string Name { get; private set; } = null!;

	[Validate]
	public string CurrencySign { get; private set; } = null!;

	[Validate]
	public string Code { get; private set; } = null!;

	public IReadOnlyCollection<CurrencyRate> RatesFrom => _ratesFrom;

	public IReadOnlyCollection<CurrencyRate> RatesTo => _ratesTo;

	public static Expression<Func<Currency, int>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Currency, bool>> GetEqualityExpression(int key) => x => x.Id == key;

	public static Currency Create(
		string name,
		string shortName,
		string currencySign,
		string code)
	{
		return new Currency(
			name,
			shortName,
			currencySign,
			code);
	}

	private void SetName(string name)
	{
		Name = name
			.Trim()
			.EnsureNotNullOrWhiteSpace(CurrencyNameNotEmptyMessage.Instance)
			.EnsureMaxLength(128, CurrencyNameMaxLengthMessage.Instance)
			.EnsureMinLength(3, CurrencyNameMinLengthMessage.Instance);
	}

	private void SetShortName(string name)
	{
		ShortName = name
			.Trim()
			.EnsureNotNullOrWhiteSpace(CurrencyShortNameNotEmptyMessage.Instance)
			.EnsureMaxLength(5, CurrencyShortNameMaxLengthMessage.Instance)
			.EnsureMinLength(2, CurrencyShortNameMinLengthMessage.Instance);
	}

	private void SetCurrencySign(string currencySign)
	{
		CurrencySign = currencySign
			.Trim()
			.EnsureNotNullOrWhiteSpace(CurrencySignNotEmptyMessage.Instance)
			.EnsureMaxLength(3, CurrencySignMaxLengthMessage.Instance)
			.EnsureMinLength(1, CurrencySignMinLengthMessage.Instance);
	}

	private void SetCode(string code)
	{
		Code = code
			.Trim()
			.EnsureNotNullOrWhiteSpace(CurrencyCodeNotEmptyMessage.Instance)
			.EnsureMaxLength(26, CurrencyCodeMaxLengthMessage.Instance)
			.EnsureMinLength(2, CurrencyCodeMinLengthMessage.Instance);
	}

	public override int GetId() => Id;
}
