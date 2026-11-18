using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Tests.AnalyticsTests;

public static class AnalyticsRunTestData
{
	public static async Task<AnalyticsRun> AddRunAsync(
		DbContext context,
		CancellationToken cancellationToken,
		string analysisType = AbcXyzRunFactory.AnalysisType,
		int ageInDays = 0,
		bool pinned = false,
		DateTime? periodStart = null,
		DateTime? periodEnd = null)
	{
		var run = AnalyticsRun.Create(
			analysisType,
			1,
			"{}",
			new string('0', 64),
			periodStart,
			periodEnd,
			null,
			null);
		if (pinned)
			run.Pin();

		await context.AddAsync(run, cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
		var createdAt = DateTime.UtcNow.AddDays(-ageInDays);
		await context.Set<AnalyticsRun>()
			.Where(item => item.Id == run.Id)
			.ExecuteUpdateAsync(setters => setters.SetProperty(item => item.CreatedAt, createdAt), cancellationToken);
		return run;
	}

	public static async Task AddPatternItemsAsync(
		DbContext context,
		Guid runId,
		CancellationToken cancellationToken,
		params (int ProductId, int Rank, DemandPattern? Pattern)[] items)
	{
		foreach (var (productId, rank, pattern) in items)
			await context.AddAsync(
				new AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>(
						productId.ToString(),
						rank,
						100m - rank,
						0.1m,
						new AbcXyzItemSegments(AbcClass.A, null, pattern),
						new AbcXyzItemData(
							rank * 2m,
							100m - rank,
							rank / 10m,
							null,
							12,
							pattern is null ? null : 4m,
							pattern is null ? null : 0.8m))
					.ToEntity(runId),
				cancellationToken);

		await context.SaveChangesAsync(cancellationToken);
	}

	public static async Task AddItemsAsync(
		DbContext context,
		Guid runId,
		CancellationToken cancellationToken,
		params (int ProductId, int Rank, AbcClass Abc, XyzClass? Xyz)[] items)
	{
		foreach (var (productId, rank, abc, xyz) in items)
			await context.AddAsync(
				new AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>(
						productId.ToString(),
						rank,
						100m - rank,
						0.1m,
						new AbcXyzItemSegments(abc, xyz),
						new AbcXyzItemData(
							rank * 2m,
							100m - rank,
							rank / 10m,
							xyz is null ? null : 0.05m,
							xyz is null ? 3 : 12))
					.ToEntity(runId),
				cancellationToken);

		await context.SaveChangesAsync(cancellationToken);
	}
}
