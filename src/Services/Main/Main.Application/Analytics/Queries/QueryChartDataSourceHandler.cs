using Main.Entities;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Exceptions;
using Main.Application.Analytics.Charts;
using Main.Application.Analytics.Interfaces.ChartData;
using Main.Entities.Exceptions.Analytics;
using NamedObject.Core.Interfaces;

namespace Main.Application.Analytics.Queries;

public sealed record QueryChartDataSourceQuery(string SystemName, string QueryInputJson)
	: IQuery<QueryChartDataSourceResult>;

public sealed record QueryChartDataSourceResult(IReadOnlyList<object> DataPoints, string? NextCursor);

public sealed class QueryChartDataSourceHandler(
	INamedObjectRegistry<ChartDataSourceNamedObject> registry,
	IJsonSerializer jsonSerializer
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
		if (jsonSerializer.TryDeserialize(json, queryInputType, out var deserialized) &&
			deserialized is IChartQueryInput chartQueryInput)
			return chartQueryInput;

		throw new InvalidInputException(ChartDataSourceQueryInputInvalidMessage.Instance);
	}
}
