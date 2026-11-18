using System.Globalization;
using System.Text.Json.Serialization;
using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain.Validation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Main.Entities.Analytics.Exceptions;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;

public sealed class AbcXyzRunItemsChartDataSource(
	IReadRepository<AnalyticsRun, Guid> runs,
	IReadRepository<AnalyticsRunItem, (Guid, string)> items,
	ISchemaGenerator schemaGenerator)
	: ChartDataSourceNamedObject<AbcXyzRunItemDataPoint, AbcXyzRunItemsChartQuery>(schemaGenerator)
{
	public const string DataSourceSystemName = nameof(AbcXyzRunItemsChartDataSource);

	private const string XyzProperty = "xyz";

	private const string DemandPatternProperty = "demandPattern";

	public override string SystemName => DataSourceSystemName;

	public override ILocalizableMessage NameLocalizationMessage
		=> ChartAbcXyzRunItemsNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> ChartAbcXyzRunItemsDescriptionMessage.Instance;

	public override async Task<ChartDataResult<AbcXyzRunItemDataPoint>> QueryAsync(
		AbcXyzRunItemsChartQuery queryInput,
		CancellationToken cancellationToken)
	{
		queryInput.EnsureNotNull(ChartAbcXyzRunItemsQueryRequiredMessage.Instance).Validate();

		var runId = queryInput.RunId;
		var runExists = await runs.Query
			.AnyAsync(run => run.Id == runId && run.AnalysisType == AbcXyzRunFactory.AnalysisType, cancellationToken);
		if (!runExists)
			throw new AnalyticsRunNotFoundException(runId);

		var query = items.Query.Where(item => item.RunId == runId);

		if (queryInput.Abc is { } abc)
		{
			var filter = AnalyticsJson.Serialize(new AbcXyzSegmentsFilter(abc, null, null));
			query = query.Where(item => EF.Functions.JsonContains(item.Segments, filter));
		}

		query = queryInput.Xyz switch
		{
			null => query,
			AbcXyzRunItemsXyzFilter.Unclassified => query.Where(item => !EF.Functions.JsonExists(
				item.Segments,
				XyzProperty)),
			{ } xyz => ApplyXyz(query, xyz)
		};

		query = queryInput.Pattern switch
		{
			null => query,
			AbcXyzRunItemsPatternFilter.Unclassified => query.Where(item => !EF.Functions.JsonExists(
				item.Segments,
				DemandPatternProperty)),
			{ } pattern => ApplyPattern(query, pattern)
		};

		var cursor = queryInput.GetCursor();
		var rows = await query.ApplyCursor(cursor).ToListAsync(cancellationToken);

		return new ChartDataResult<AbcXyzRunItemDataPoint>(
			rows.Select(ToPoint).ToList(),
			rows.GetNextCursor(cursor));
	}

	private static IQueryable<AnalyticsRunItem> ApplyPattern(
		IQueryable<AnalyticsRunItem> query,
		AbcXyzRunItemsPatternFilter pattern)
	{
		var filter = AnalyticsJson.Serialize(
			new AbcXyzSegmentsFilter(null, null, Enum.Parse<DemandPattern>(pattern.ToString())));
		return query.Where(item => EF.Functions.JsonContains(item.Segments, filter));
	}

	private static IQueryable<AnalyticsRunItem> ApplyXyz(
		IQueryable<AnalyticsRunItem> query,
		AbcXyzRunItemsXyzFilter xyz)
	{
		var filter = AnalyticsJson.Serialize(new AbcXyzSegmentsFilter(null, Enum.Parse<XyzClass>(xyz.ToString()), null));
		return query.Where(item => EF.Functions.JsonContains(item.Segments, filter));
	}

	private static AbcXyzRunItemDataPoint ToPoint(AnalyticsRunItem item)
	{
		var model = AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>.From(item);
		return new AbcXyzRunItemDataPoint
		{
			ProductId = int.Parse(model.SubjectKey, CultureInfo.InvariantCulture),
			Rank = model.Rank ?? 0,
			Value = model.Value ?? 0m,
			Share = model.Share ?? 0m,
			CumulativeShare = model.Data.CumulativeShare,
			Abc = model.Segments.Abc,
			Xyz = model.Segments.Xyz,
			Quantity = model.Data.Quantity,
			Revenue = model.Data.Revenue,
			CoefficientOfVariation = model.Data.CoefficientOfVariation,
			PeriodsCount = model.Data.PeriodsCount,
			DemandPattern = model.Segments.DemandPattern,
			Adi = model.Data.Adi,
			Cv2 = model.Data.Cv2
		};
	}

	private sealed record AbcXyzSegmentsFilter(
		[property: JsonConverter(typeof(JsonStringEnumConverter))]
		AbcClass? Abc,
		[property: JsonConverter(typeof(JsonStringEnumConverter))]
		XyzClass? Xyz,
		[property: JsonConverter(typeof(JsonStringEnumConverter))]
		DemandPattern? DemandPattern);
}
