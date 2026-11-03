using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Exceptions;
using Main.Enums;

namespace Main.Entities.Storage;

public class StorageContent : AuditableEntity<StorageContent, int>,
	ILinqEntity<StorageContent, int>,
	IVersionable<uint>
{
	private readonly List<StorageContentLock> _locks = [];
	private bool _locksLoaded;

	private StorageContent()
	{
	}

	private StorageContent(
		string storageCode,
		int productId,
		decimal buyPrice,
		int currencyId,
		decimal buyPriceInBaseCurrency,
		int buyPriceInBaseCurrencyId,
		DateTime purchaseDatetime)
	{
		_locksLoaded = true;
		StorageCode = storageCode;
		ProductId = productId;
		PurchaseDatetime = purchaseDatetime;
		SetCurrencyId(currencyId);
		SetBaseCurrencyId(buyPriceInBaseCurrencyId);
		SetBuyPrice(buyPrice, buyPriceInBaseCurrency);
	}

	[Validate]
	public int Id { get; private set; }

	public string StorageCode { get; } = null!;

	public int ProductId { get; }

	public int Count { get; private set; }

	public decimal BuyPrice { get; private set; }

	public decimal BuyPriceInBaseCurrency { get; private set; }

	public int BaseCurrencyId { get; private set; }

	public int CurrencyId { get; private set; }

	public DateTime PurchaseDatetime { get; private set; }

	public Currency.Currency Currency { get; private set; } = null!;

	public IReadOnlyCollection<StorageContentLock> Locks => _locks;

	public static Expression<Func<StorageContent, int>> GetKeySelector() => x => x.Id;

	public static Expression<Func<StorageContent, bool>> GetEqualityExpression(int key) => x => x.Id == key;

	public uint RowVersion { get; private set; }

	public static StorageContent Create(
		string storageCode,
		int productId,
		decimal buyPrice,
		int currencyId,
		decimal buyPriceInBaseCurrency,
		int buyPriceInBaseCurrencyId,
		DateTime purchaseDatetime)
	{
		return new StorageContent(
			storageCode,
			productId,
			buyPrice,
			currencyId,
			buyPriceInBaseCurrency,
			buyPriceInBaseCurrencyId,
			purchaseDatetime);
	}

	public void SetCount(int count, StorageMovementType movementType, DateTime? utcNow = null)
	{
		var newCount = count.EnsureNonNegative(() =>
			new InvalidOperationException("Count must be greater than or equal to zero."));

		if (Count == newCount)
			return;

		if (newCount < Count)
		{
			var available = GetAvailableQuantity(utcNow ?? DateTime.UtcNow);
			if (Count - newCount > available)
				throw new NotEnoughCountOnStorageException(ProductId, available, Count - newCount);
		}

		AddDomainEvent(
			new StorageContentCountUpdatedDomainEvent(
				ProductId,
				StorageCode,
				CurrencyId,
				newCount,
				BuyPrice,
				movementType,
				newCount - Count));

		Count = newCount;
	}

	public void IncreaseCount(int amount, StorageMovementType movementType) =>
		SetCount(checked(Count + amount), movementType);

	public void MarkLocksLoaded() => _locksLoaded = true;

	public int GetAvailableQuantity(DateTime utcNow)
	{
		if (!_locksLoaded)
			throw new InvalidOperationException("Storage content locks must be loaded before checking availability.");
		if (utcNow.Kind != DateTimeKind.Utc)
			throw new InvalidOperationException("Availability must be checked using UTC time.");

		var locked = _locks.Where(item => item.IsActiveAt(utcNow)).Sum(item => (long)item.Quantity);
		return (int)Math.Max(0L, Count - locked);
	}

	public void Take(int quantity, StorageMovementType movementType, DateTime utcNow)
	{
		if (quantity <= 0)
			throw new InvalidOperationException("Quantity to take must be greater than zero.");
		var available = GetAvailableQuantity(utcNow);
		if (quantity > available)
			throw new NotEnoughCountOnStorageException(ProductId, available, quantity);
		SetCount(Count - quantity, movementType, utcNow);
	}

	public void SetBuyPrice(decimal buyPrice, decimal buyPriceInBaseCurrency)
	{
		buyPrice
			.EnsureMaxDecimalPlaces(
				2,
				() => new InvalidOperationException("Buy price must have maximum 2 decimal places."))
			.EnsureAtLeast(0.001m, () => new InvalidOperationException("Buy price must be grater then 0."));

		buyPriceInBaseCurrency.EnsureGreaterThan(
			0,
			() => new InvalidOperationException("Buy price in base currency must be greater then 0."));

		BuyPrice = buyPrice;
		BuyPriceInBaseCurrency = buyPriceInBaseCurrency;
	}
	public override void OnDeleted()
	{
		AddDomainEvent(
			new StorageContentCountUpdatedDomainEvent(
				ProductId,
				StorageCode,
				CurrencyId,
				0,
				BuyPrice,
				StorageMovementType.StorageContentDeletion,
				-Count));
		AddDomainEvent(new StorageContentUpdatedDomainEvent(this, true));
	}

	public override void OnUpdated() => AddDomainEvent(new StorageContentUpdatedDomainEvent(this, false));

	public override void OnCreated() => AddDomainEvent(new StorageContentUpdatedDomainEvent(this, false));

	public void SetCurrencyId(int currencyId) => CurrencyId = currencyId;

	public void AssignCurrency(Currency.Currency currency)
	{
		CurrencyId = currency.Id;
		Currency = currency;
	}

	public void SetBaseCurrencyId(int baseCurrencyId) => BaseCurrencyId = baseCurrencyId;

	public void SetPurchaseDate(DateTime purchaseDate) => PurchaseDatetime = purchaseDate;

	public override int GetId() => Id;
}
