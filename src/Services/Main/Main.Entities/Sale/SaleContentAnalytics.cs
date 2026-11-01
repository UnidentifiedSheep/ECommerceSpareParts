using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Sale;

public class SaleContentAnalytics : Entity<SaleContentAnalytics, int>, ILinqEntity<SaleContentAnalytics, int>
{
	private SaleContentAnalytics() { }

	public int SaleContentId { get; private set; }
	public decimal OriginalAmountInBaseCurrency { get; private set; }
	public decimal RevenueInBaseCurrency { get; private set; }
	public decimal CostInBaseCurrency { get; private set; }
	public SaleContent Content { get; private set; } = null!;

	public static SaleContentAnalytics Create(int saleContentId, decimal originalAmount, decimal revenue, decimal cost)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(saleContentId);
		var result = new SaleContentAnalytics { SaleContentId = saleContentId };
		result.Update(originalAmount, revenue, cost);
		return result;
	}

	public void Update(decimal originalAmount, decimal revenue, decimal cost)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(originalAmount);
		ArgumentOutOfRangeException.ThrowIfNegative(revenue);
		ArgumentOutOfRangeException.ThrowIfNegative(cost);
		if (revenue > originalAmount)
			throw new InvalidOperationException("Revenue cannot exceed the amount before discount.");
		OriginalAmountInBaseCurrency = Math.Round(originalAmount, 12);
		RevenueInBaseCurrency = Math.Round(revenue, 12);
		CostInBaseCurrency = Math.Round(cost, 12);
	}

	public override int GetId() => SaleContentId;
	public static Expression<Func<SaleContentAnalytics, int>> GetKeySelector() => item => item.SaleContentId;
	public static Expression<Func<SaleContentAnalytics, bool>> GetEqualityExpression(int key) => item => item.SaleContentId == key;
}
