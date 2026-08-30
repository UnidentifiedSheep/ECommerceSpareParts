using Analytics.Entities;
using Bogus;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.DataBuilders.Sale;

public sealed class SaleContentBuilder(Faker faker) : BuilderBase<SaleContent>(faker)
{
	private readonly List<SaleContentDetail> _details = [];

	private int? _count;

	private decimal? _discount;

	private int? _id;

	private decimal? _price;

	private decimal? _priceInBaseCurrency;

	private int? _productId;

	private Guid? _saleId;

	public SaleContentBuilder WithId(int id)
	{
		_id = id;
		return this;
	}

	public SaleContentBuilder WithSaleId(Guid saleId)
	{
		_saleId = saleId;
		return this;
	}

	public SaleContentBuilder WithProductId(int productId)
	{
		_productId = productId;
		return this;
	}

	public SaleContentBuilder WithPrice(decimal price)
	{
		_price = price;
		return this;
	}

	public SaleContentBuilder WithPriceInBaseCurrency(decimal price)
	{
		_priceInBaseCurrency = price;
		return this;
	}

	public SaleContentBuilder WithCount(int count)
	{
		_count = count;
		return this;
	}

	public SaleContentBuilder WithDiscount(decimal discount)
	{
		_discount = discount;
		return this;
	}

	public SaleContentBuilder WithDetails(IEnumerable<SaleContentDetail> details)
	{
		_details.Clear();
		_details.AddRange(details);
		return this;
	}

	public override SaleContent Build()
	{
		var id = _id ?? Faker.Random.Int(1);
		var count = _count ?? 1;
		var price = _price ?? Math.Round(Faker.Random.Decimal(1m, 1000m), 2);
		var details = _details.Count > 0
			? _details
			: [new SaleContentDetailBuilder(Faker).WithSaleContentId(id).WithCount(count).Build()];

		return SaleContent.Create(
			id,
			_saleId ?? Guid.NewGuid(),
			_productId ?? Faker.Random.Int(1),
			price,
			_priceInBaseCurrency ?? price,
			count,
			_discount ?? 0m,
			details);
	}
}
