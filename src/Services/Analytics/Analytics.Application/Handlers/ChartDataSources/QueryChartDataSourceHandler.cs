using System.Text.Json;
using Abstractions.Interfaces;
using Analytics.Application.Interfaces.ChartData;
using Analytics.Application.NamedObjects.ChartDataSources;
using Analytics.Entities.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Exceptions;
using Exceptions.Base.Localized;

namespace Analytics.Application.Handlers.ChartDataSources;

public sealed record QueryChartDataSourceQuery(
    string SystemName,
    string QueryInputJson
) : IQuery<QueryChartDataSourceResult>;

public sealed record QueryChartDataSourceResult(IReadOnlyList<object> DataPoints);

public sealed class QueryChartDataSourceHandler(
    INamedObjectRegistry<ChartDataSourceNamedObject> registry,
    IJsonSerializer jsonSerializer
) : IQueryHandler<QueryChartDataSourceQuery, QueryChartDataSourceResult>
{
    public async Task<QueryChartDataSourceResult> Handle(
        QueryChartDataSourceQuery request,
        CancellationToken cancellationToken)
    {
        var dataSource = registry.TryGetBySystemName(request.SystemName)
                         ?? throw new ChartDataSourceNotFoundException(request.SystemName);

        var queryInput = DeserializeQueryInput(
            request.QueryInputJson,
            dataSource.QueryInputType);

        var dataPoints = await dataSource
            .QueryAsync(queryInput, cancellationToken);

        return new QueryChartDataSourceResult(
            dataPoints.Cast<object>().ToList());
    }

    private IChartQueryInput DeserializeQueryInput(
        string json,
        Type queryInputType)
    {
        try
        {
            return jsonSerializer.Deserialize(json, queryInputType) as IChartQueryInput
                   ?? throw new InvalidInputException("chart.data.source.query.input.invalid");
        }
        catch (JsonException)
        {
            throw new InvalidInputException("chart.data.source.query.input.invalid");
        }
    }
}