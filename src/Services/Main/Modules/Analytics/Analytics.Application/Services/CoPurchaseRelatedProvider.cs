using System.Globalization;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Analytics;
using Main.Entities.Analytics.Exceptions;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Analytics.Application.Services;

public interface ICoPurchaseRelatedProvider
{
	Task<CoPurchaseRelatedLookup?> GetAsync(
		int productId,
		Guid? runId,
		CancellationToken cancellationToken = default);
}

public sealed class CoPurchaseRelatedProvider(
	IReadRepository<AnalyticsRun, Guid> runs,
	IReadRepository<AnalyticsRunItem, (Guid, string)> items,
	IProductCrosses productCrosses) : ICoPurchaseRelatedProvider
{
	private static readonly string HasRelatedFilter = AnalyticsJson.Serialize(new CoPurchaseItemSegments(true));

	public async Task<CoPurchaseRelatedLookup?> GetAsync(
		int productId,
		Guid? runId,
		CancellationToken cancellationToken = default)
	{
		var resolvedRunId = await ResolveRunIdAsync(runId, cancellationToken);
		if (resolvedRunId is not { } id)
			return null;

		var productKey = Key(productId);
		var own = await items.Query
			.Where(item => item.RunId == id &&
				item.SubjectKey == productKey &&
				EF.Functions.JsonContains(item.Segments, HasRelatedFilter))
			.FirstOrDefaultAsync(cancellationToken);
		if (own is not null)
			return ToLookup(id, own);

		var crossKeys = (await productCrosses.GetCrossIdsAsync(productId, cancellationToken))
			.Select(Key)
			.ToList();
		if (crossKeys.Count == 0)
			return null;

		var cross = await items.Query
			.Where(item => item.RunId == id &&
				crossKeys.Contains(item.SubjectKey) &&
				EF.Functions.JsonContains(item.Segments, HasRelatedFilter))
			.OrderByDescending(item => item.Value)
			.ThenBy(item => item.Rank)
			.FirstOrDefaultAsync(cancellationToken);

		return cross is null ? null : ToLookup(id, cross);
	}

	private async Task<Guid?> ResolveRunIdAsync(Guid? runId, CancellationToken cancellationToken)
	{
		if (runId is { } requested)
		{
			var exists = await runs.Query.AnyAsync(
				run => run.Id == requested &&
					run.AnalysisType == CoPurchaseRunFactory.AnalysisType &&
					run.CompletedAt != null,
				cancellationToken);
			return exists ? requested : throw new AnalyticsRunNotFoundException(requested);
		}

		return await runs.Query
			.Where(run =>
				run.AnalysisType == CoPurchaseRunFactory.AnalysisType &&
				run.CompletedAt != null)
			.OrderByDescending(run => run.IsPinned)
			.ThenByDescending(run => run.CompletedAt)
			.Select(run => (Guid?)run.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	private static CoPurchaseRelatedLookup ToLookup(Guid runId, AnalyticsRunItem item)
		=> new(
			runId,
			int.Parse(item.SubjectKey, CultureInfo.InvariantCulture),
			AnalyticsRunItemModel<CoPurchaseItemSegments, CoPurchaseItemData>.From(item).Data.Related);

	private static string Key(int productId) => productId.ToString(CultureInfo.InvariantCulture);
}
