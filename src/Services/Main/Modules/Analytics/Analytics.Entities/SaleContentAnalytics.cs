using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Analytics;

public class SaleContentAnalytics : Entity<SaleContentAnalytics, int>, ILinqEntity<SaleContentAnalytics, int>
{
	private SaleContentAnalytics() { }

	public int SaleContentId { get; private set; }
	public Guid SaleId { get; private set; }
	public Guid OrganizationId { get; private set; }
	public Guid BuyerId { get; private set; }
	public DateTime SaleDatetime { get; private set; }
	public bool IsCompleted { get; private set; }
	public int Count { get; private set; }
	public decimal OriginalAmountInBaseCurrency { get; private set; }
	public decimal RevenueInBaseCurrency { get; private set; }
	public decimal CostInBaseCurrency { get; private set; }

	public static Expression<Func<SaleContentAnalytics, int>> GetKeySelector() => item => item.SaleContentId;

	public static Expression<Func<SaleContentAnalytics, bool>> GetEqualityExpression(int key)
		=> item => item.SaleContentId == key;

	public static SaleContentAnalytics Create(
		int saleContentId,
		Guid saleId,
		Guid organizationId,
		Guid buyerId,
		DateTime saleDatetime,
		int count,
		decimal originalAmount,
		decimal revenue,
		decimal cost)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(saleContentId);
		var result = new SaleContentAnalytics { SaleContentId = saleContentId };
		result.Update(
			saleId,
			organizationId,
			buyerId,
			saleDatetime,
			count,
			originalAmount,
			revenue,
			cost);
		return result;
	}

	public void Update(
		Guid saleId,
		Guid organizationId,
		Guid buyerId,
		DateTime saleDatetime,
		int count,
		decimal originalAmount,
		decimal revenue,
		decimal cost)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
		ArgumentOutOfRangeException.ThrowIfNegative(originalAmount);
		ArgumentOutOfRangeException.ThrowIfNegative(revenue);
		ArgumentOutOfRangeException.ThrowIfNegative(cost);
		if (revenue > originalAmount)
			throw new InvalidOperationException("Revenue cannot exceed the amount before discount.");
		SaleId = saleId;
		OrganizationId = organizationId;
		BuyerId = buyerId;
		SaleDatetime = saleDatetime;
		Count = count;
		IsCompleted = true;
		OriginalAmountInBaseCurrency = Math.Round(originalAmount, 12);
		RevenueInBaseCurrency = Math.Round(revenue, 12);
		CostInBaseCurrency = Math.Round(cost, 12);
	}

	public void Deactivate() => IsCompleted = false;

	public override int GetId() => SaleContentId;
}
