using Analytics.Entities;
using Bogus;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.DataBuilders.Sale;

public sealed class SalesFactBuilder(Faker faker) : BuilderBase<SalesFact>(faker)
{
	private readonly List<SaleContent> _contents = [];

	private int? _baseCurrencyId;

	private Guid? _buyerId;

	private DateTime? _createdAt;

	private int? _currencyId;

	private bool _deleted;

	private Guid? _id;

	private Guid? _organizationId;

	private DateTime? _processedAt;

	public SalesFactBuilder WithId(Guid id)
	{
		_id = id;
		return this;
	}

	public SalesFactBuilder WithCurrencyId(int currencyId)
	{
		_currencyId = currencyId;
		return this;
	}

	public SalesFactBuilder WithBaseCurrencyId(int baseCurrencyId)
	{
		_baseCurrencyId = baseCurrencyId;
		return this;
	}

	public SalesFactBuilder WithOrganizationId(Guid organizationId)
	{
		_organizationId = organizationId;
		return this;
	}

	public SalesFactBuilder WithBuyerId(Guid buyerId)
	{
		_buyerId = buyerId;
		return this;
	}

	public SalesFactBuilder WithCreatedAt(DateTime createdAt)
	{
		_createdAt = createdAt;
		return this;
	}

	public SalesFactBuilder WithProcessedAt(DateTime processedAt)
	{
		_processedAt = processedAt;
		return this;
	}

	public SalesFactBuilder WithContents(IEnumerable<SaleContent> contents)
	{
		_contents.Clear();
		_contents.AddRange(contents);
		return this;
	}

	public SalesFactBuilder Deleted()
	{
		_deleted = true;
		return this;
	}

	public override SalesFact Build()
	{
		var id = _id ?? Guid.NewGuid();
		var processedAt = _processedAt ?? DateTime.UtcNow;

		if (_deleted)
			return SalesFact.CreateDeleted(id, processedAt);

		var contents = _contents.Count > 0
			? _contents
			: [new SaleContentBuilder(Faker).WithSaleId(id).Build()];

		return SalesFact.Create(
			id,
			_currencyId ?? Faker.Random.Int(1),
			_baseCurrencyId ?? Faker.Random.Int(1),
			_organizationId ?? Guid.NewGuid(),
			_buyerId ?? Guid.NewGuid(),
			_createdAt ?? DateTime.UtcNow,
			processedAt,
			contents);
	}
}
