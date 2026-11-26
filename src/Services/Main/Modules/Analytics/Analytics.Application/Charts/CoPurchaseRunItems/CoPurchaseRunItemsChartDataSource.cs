using System.Globalization;
using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain.Validation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Main.Entities.Analytics.Exceptions;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRunItems;

public sealed class CoPurchaseRunItemsChartDataSource(
	IReadRepository<AnalyticsRun, Guid> runs,
	IReadRepository<AnalyticsRunItem, (Guid, string)> items,
	ISchemaGenerator schemaGenerator)
	: ChartDataSourceNamedObject<CoPurchaseRunItemDataPoint, CoPurchaseRunItemsChartQuery>(schemaGenerator)
{
	public const string DataSourceSystemName = nameof(CoPurchaseRunItemsChartDataSource);

	public override string SystemName => DataSourceSystemName;

	public override ILocalizableMessage NameLocalizationMessage => ChartCoPurchaseRunItemsNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> ChartCoPurchaseRunItemsDescriptionMessage.Instance;

	public override async Task<ChartDataResult<CoPurchaseRunItemDataPoint>> QueryAsync(
		CoPurchaseRunItemsChartQuery queryInput,
		CancellationToken cancellationToken)
	{
		queryInput.EnsureNotNull(ChartCoPurchaseRunItemsQueryRequiredMessage.Instance).Validate();
		var runId = queryInput.RunId;
		var runExists = await runs.Query
			.AnyAsync(
				run => run.Id == runId && run.AnalysisType == CoPurchaseRunFactory.AnalysisType,
				cancellationToken);
		if (!runExists)
			throw new AnalyticsRunNotFoundException(runId);

		var query = items.Query.Where(item => item.RunId == runId);
		if (queryInput.HasRelated is { } hasRelated)
		{
			var filter = AnalyticsJson.Serialize(new CoPurchaseItemSegments(hasRelated));
			query = query.Where(item => EF.Functions.JsonContains(item.Segments, filter));
		}

		var cursor = queryInput.GetCursor();
		var rows = await query.ApplyCursor(cursor).ToListAsync(cancellationToken);
		return new ChartDataResult<CoPurchaseRunItemDataPoint>(
			rows.Select(ToPoint).ToList(),
			rows.GetNextCursor(cursor));
	}

	private static CoPurchaseRunItemDataPoint ToPoint(AnalyticsRunItem item)
	{
		var data = AnalyticsRunItemModel<CoPurchaseItemSegments, CoPurchaseItemData>.From(item).Data;
		var top = data.Related.Count > 0 ? data.Related[0] : null;
		return new CoPurchaseRunItemDataPoint
		{
			ProductId = int.Parse(item.SubjectKey, CultureInfo.InvariantCulture),
			SalesCount = data.SalesCount,
			RelatedCount = data.Related.Count,
			TopRelatedProductId = top?.Members[0].ProductId,
			TopConfidence = top?.Confidence,
			TopLift = top?.Lift
		};
	}
}
