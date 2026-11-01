using Main.Entities.DomainEvents.Sale;
using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Sale;

public class SaleContent : Entity<SaleContent, int>, ILinqEntity<SaleContent, int>
{
	private readonly List<SaleContentDetail> _details = [];

	private SaleContent()
	{
	}

	private SaleContent(
		int productId,
		decimal priceWithOutDiscount,
		decimal priceWithDiscount,
		IEnumerable<SaleContentDetail> details)
	{
		ProductId = productId;
		SetPriceAndDetails(
			priceWithOutDiscount,
			priceWithDiscount,
			details);
	}

	public int Id { get; private set; }

	public Guid SaleId { get; private set; }

	public int ProductId { get; private set; }

	public int Count { get; private set; }

	public decimal OriginalPrice { get; private set; }
	public decimal DiscountedPrice { get; private set; }

	public decimal TotalSum { get; private set; }

	public SaleContentAnalytics? Analytics { get; private set; }

	public string? Comment { get; private set; }

	public Product.Product Product { get; private set; } = null!;

	public Sale Sale { get; private set; } = null!;

	public IReadOnlyList<SaleContentDetail> Details => _details;

	public static Expression<Func<SaleContent, int>> GetKeySelector() => x => x.Id;

	public static Expression<Func<SaleContent, bool>> GetEqualityExpression(int key) => x => x.Id == key;

	public static SaleContent Create(
		int productId,
		decimal priceWithOutDiscount,
		decimal priceWithDiscount,
		IEnumerable<SaleContentDetail> details)
	{
		return new SaleContent(
			productId,
			priceWithOutDiscount,
			priceWithDiscount,
			details);
	}

	private void SetCount(int count) =>
		Count = count.EnsureGreaterThan(0, SaleContentCountMinMessage.Instance);

	public void SetPriceAndDetails(
		decimal originalPrice,
		decimal discountedPrice,
		IEnumerable<SaleContentDetail> details)
	{
		originalPrice
			.EnsureMaxDecimalPlaces(2, SaleContentPricePrecisionMessage.Instance)
			.EnsureGreaterThan(0, SaleContentPriceMinMessage.Instance);

		discountedPrice
			.EnsureMaxDecimalPlaces(2, SaleContentPriceWithDiscountPrecisionMessage.Instance)
			.EnsureGreaterThan(0, SaleContentPriceWithDiscountMinMessage.Instance)
			.EnsureAtMost(originalPrice, SaleContentPriceWithDiscountMaxMessage.Instance);

		var nextDetails = details.ToList();
		var financialDataChanged = OriginalPrice != originalPrice || DiscountedPrice != discountedPrice ||
			!_details.Select(detail => (detail.StorageContentId, detail.CurrencyId, detail.BuyPrice, detail.Count, detail.PurchaseDatetime))
				.OrderBy(value => value).SequenceEqual(nextDetails.Select(detail =>
					(detail.StorageContentId, detail.CurrencyId, detail.BuyPrice, detail.Count, detail.PurchaseDatetime)).OrderBy(value => value));
		OriginalPrice = originalPrice;
		DiscountedPrice = discountedPrice;

		ClearAndSetDetails(nextDetails);
		TotalSum = DiscountedPrice * Count;
		if (financialDataChanged)
			InvalidateAnalytics();
	}

	private void ClearAndSetDetails(IEnumerable<SaleContentDetail> details)
	{
		var list = details.ToList();
		var detailsCount = list.Sum(detail => detail.Count);

		SetCount(detailsCount);

		_details.Clear();
		_details.AddRange(list);
	}

	internal void SetSaleId(Guid saleId) => SaleId = saleId;

	public void SetComment(string? comment)
	{
		comment = comment.NullIfWhiteSpace()?.EnsureMaxLength(256, SaleContentCommentMaxMessage.Instance);
		if (Comment == comment) return;
		Comment = comment;
		NotifyUpdated(false);
	}

	public void UpdateAnalytics(decimal originalAmount, decimal revenue, decimal cost)
	{
		if (Analytics is null)
			Analytics = SaleContentAnalytics.Create(Id, originalAmount, revenue, cost);
		else
			Analytics.Update(originalAmount, revenue, cost);
	}

	public void InvalidateAnalytics() => NotifyUpdated(true);

	public override void OnCreated() => InvalidateAnalytics();


	private void NotifyUpdated(bool financialInfoChanged)
	{
		if (SaleId != Guid.Empty)
			AddDomainEvent(new SaleContentUpdateDomainEvent(this, financialInfoChanged));
	}

	public override int GetId() => Id;
}
