using Application.Common.Interfaces.Repositories;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Sale.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Analytics.Application.Services;

public sealed class AbcXyzDataReader(
	IReadRepository<SaleContentAnalytics, int> analytics,
	ISaleContentSource contents)
{
	public async Task<IReadOnlyList<ProductPeriodValue>> ReadAsync(
		AbcXyzParameters parameters,
		CancellationToken cancellationToken)
		=> (await BuildQuery(parameters).ToListAsync(cancellationToken))
			.Select(row => new ProductPeriodValue(
				row.ProductId,
				new DateTime(row.Year, row.Month, 1, 0, 0, 0, DateTimeKind.Utc),
				row.Quantity,
				row.Revenue))
			.ToList();

	public IQueryable<ProductPeriodRow> BuildQuery(AbcXyzParameters parameters)
	{
		var from = parameters.From;
		var to = parameters.EndExclusive();
		var months = parameters.Months?.Select(month => (int)month).ToArray() ?? [];

		var rows = analytics.Query
			.Where(row => row.IsCompleted && row.SaleDatetime >= from && row.SaleDatetime < to);

		if (parameters.OrganizationId is { } organizationId)
			rows = rows.Where(row => row.OrganizationId == organizationId);
		if (parameters.BuyerId is { } buyerId)
			rows = rows.Where(row => row.BuyerId == buyerId);
		if (months.Length > 0)
			rows = rows.Where(row => months.Contains(row.SaleDatetime.Month));

		return rows
			.Join(
				contents.Query,
				row => row.SaleContentId,
				content => content.SaleContentId,
				(row, content) => new
				{
					content.ProductId,
					row.SaleDatetime.Year,
					row.SaleDatetime.Month,
					row.Count,
					row.RevenueInBaseCurrency
				})
			.GroupBy(row => new { row.ProductId, row.Year, row.Month })
			.Select(group => new ProductPeriodRow
			{
				ProductId = group.Key.ProductId,
				Year = group.Key.Year,
				Month = group.Key.Month,
				Quantity = group.Sum(row => (decimal)row.Count),
				Revenue = group.Sum(row => row.RevenueInBaseCurrency)
			});
	}

	public sealed class ProductPeriodRow
	{
		public int ProductId { get; init; }

		public int Year { get; init; }

		public int Month { get; init; }

		public decimal Quantity { get; init; }

		public decimal Revenue { get; init; }
	}
}
