using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;

namespace Pricing.Entities.Offers;

public class ProductPriceOption : AuditableEntity<ProductPriceOption, Guid>,
	ILinqEntity<ProductPriceOption, Guid>
{
	private ProductPriceOption()
	{
	}

	public Guid PriceOfferId { get; private set; }

	public decimal Score { get; private set; }

	public int CurrencyId { get; private set; }

	public decimal Price { get; private set; }

	public decimal Markup { get; private set; }

	public string ForStorageCode { get; private set; } = string.Empty;

	public int DeliveryDays { get; private set; }

	public int GuaranteedDeliveryDays { get; private set; }

	public int DeliveryProbability { get; private set; }

	public string MarkupVersion { get; private set; } = string.Empty;

	public string AppliersVersion { get; private set; } = string.Empty;

	public Guid PricingSettingsVersion { get; private set; }

	public PriceOffer PriceOffer { get; private set; } = null!;

	public static Expression<Func<ProductPriceOption, Guid>> GetKeySelector() => x => x.PriceOfferId;

	public static Expression<Func<ProductPriceOption, bool>> GetEqualityExpression(Guid key)
		=> x =>
			x.PriceOfferId == key;

	public static ProductPriceOption Create(
		Guid priceOfferId,
		string markupVersion,
		string appliersVersion,
		Guid pricingSettingsVersion,
		string storageCode,
		decimal score,
		decimal price,
		int currencyId,
		decimal markup,
		int deliveryDays,
		int guaranteedDeliveryDays,
		int deliveryProbability)
	{
		var option = new ProductPriceOption
		{
			PriceOfferId = priceOfferId, CurrencyId = currencyId
		};
		option.SetStorageCode(storageCode);
		option.SetScore(score);
		option.SetPrice(price);
		option.SetMarkup(markup);
		option.SetDeliveryDays(
			deliveryDays,
			guaranteedDeliveryDays,
			deliveryProbability);
		option.MarkupVersion = markupVersion;
		option.AppliersVersion = appliersVersion;
		option.PricingSettingsVersion = pricingSettingsVersion;
		return option;
	}

	public void SetScore(decimal score)
		=> Score =
			score.EnsureNonNegative(() => new InvalidOperationException("Score cannot be negative"));

	public void SetPrice(decimal priceInBaseCurrency)
		=> Price = priceInBaseCurrency.EnsureGreaterThan(
			0,
			() => new InvalidOperationException("Price cannot be negative"));

	public void SetMarkup(decimal markup)
		=> Markup =
			markup.EnsureNonNegative(() => new InvalidOperationException("Markup cannot be negative"));

	public void SetDeliveryDays(
		int deliveryDays,
		int guaranteedDeliveryDays,
		int deliveryProbability)
	{
		deliveryProbability.EnsureNonNegative(() =>
			new InvalidOperationException("Delivery probability cannot be negative"));
		deliveryDays.EnsureNonNegative(() =>
			new InvalidOperationException("Delivery days cannot be negative"));
		guaranteedDeliveryDays.EnsureNonNegative(() =>
			new InvalidOperationException("Guaranteed delivery days cannot be negative"));

		DeliveryDays = deliveryDays;
		GuaranteedDeliveryDays = guaranteedDeliveryDays;
		DeliveryProbability = deliveryProbability;
	}

	public void SetStorageCode(string storageCode)
		=> ForStorageCode = storageCode
			.TrimSafe()
			.EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("Storage name cannot be empty"));

	public override Guid GetId() => PriceOfferId;
}
