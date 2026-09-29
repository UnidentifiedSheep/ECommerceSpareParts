using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Models.Options;
using Analytics.Application.Interfaces.ChartData;
using Analytics.Application.NamedObjects.ChartDataSources;
using Analytics.Entities;
using Analytics.Entities.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Exceptions;
using Extensions;
using NamedObject.Core.Interfaces;

namespace Analytics.Application.Handlers.ChartDataSources;

public sealed record QueryChartDataSourceQuery(string SystemName, string QueryInputJson)
	: IQuery<QueryChartDataSourceResult>;

public sealed record QueryChartDataSourceResult(IReadOnlyList<object> DataPoints, string? NextCursor);

public sealed class QueryChartDataSourceHandler(
	INamedObjectRegistry<ChartDataSourceNamedObject> registry,
	ProjectJsonOptions jsonOptions
	) : IQueryHandler<QueryChartDataSourceQuery, QueryChartDataSourceResult>
{
	public async Task<QueryChartDataSourceResult> Handle(
		QueryChartDataSourceQuery request,
		CancellationToken cancellationToken)
	{
		var dataSource = registry.TryGetBySystemName(request.SystemName) ??
			throw new ChartDataSourceNotFoundException(request.SystemName);

		var queryInput = DeserializeQueryInput(request.QueryInputJson, dataSource.QueryInputType);

		var result = await dataSource.QueryAsync(queryInput, cancellationToken);

		return new QueryChartDataSourceResult(result.DataPoints.Cast<object>().ToList(), result.NextCursor);
	}

	private IChartQueryInput DeserializeQueryInput(string json, Type queryInputType)
	{
		if (json.TryDeserializeJson(queryInputType, out var deserialized, jsonOptions.SerializerOptions) &&
			deserialized is IChartQueryInput chartQueryInput)
			return chartQueryInput;

		throw new InvalidInputException(ChartDataSourceQueryInputInvalidMessage.Instance);
	}
}
