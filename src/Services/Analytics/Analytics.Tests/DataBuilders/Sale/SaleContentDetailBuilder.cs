using Analytics.Entities;
using Bogus;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.DataBuilders.Sale;

public sealed class SaleContentDetailBuilder(Faker faker) : BuilderBase<SaleContentDetail>(faker)
{
	private decimal? _buyPrice;

	private decimal? _buyPriceInBaseCurrency;

	private int? _count;

	private int? _currencyId;

	private int? _id;

	private DateTime? _purchaseDate;

	private int? _saleContentId;

	public SaleContentDetailBuilder WithId(int id)
	{
		_id = id;
		return this;
	}

	public SaleContentDetailBuilder WithSaleContentId(int saleContentId)
	{
		_saleContentId = saleContentId;
		return this;
	}

	public SaleContentDetailBuilder WithCurrencyId(int currencyId)
	{
		_currencyId = currencyId;
		return this;
	}

	public SaleContentDetailBuilder WithBuyPrice(decimal buyPrice)
	{
		_buyPrice = buyPrice;
		return this;
	}

	public SaleContentDetailBuilder WithBuyPriceInBaseCurrency(decimal buyPrice)
	{
		_buyPriceInBaseCurrency = buyPrice;
		return this;
	}

	public SaleContentDetailBuilder WithCount(int count)
	{
		_count = count;
		return this;
	}

	public SaleContentDetailBuilder WithPurchaseDate(DateTime purchaseDate)
	{
		_purchaseDate = purchaseDate;
		return this;
	}

	public override SaleContentDetail Build()
	{
		var buyPrice = _buyPrice ?? Math.Round(Faker.Random.Decimal(1m, 1000m), 2);

		return SaleContentDetail.Create(
			_id ?? Faker.Random.Int(1),
			_saleContentId ?? Faker.Random.Int(1),
			_currencyId ?? Faker.Random.Int(1),
			buyPrice,
			_buyPriceInBaseCurrency ?? buyPrice,
			_count ?? Faker.Random.Int(1, 100),
			_purchaseDate ?? Faker.Date.Recent().ToUniversalTime());
	}
}
