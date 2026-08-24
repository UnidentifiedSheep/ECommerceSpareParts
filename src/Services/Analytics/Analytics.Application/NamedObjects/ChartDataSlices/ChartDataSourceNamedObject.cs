using Analytics.Application.Interfaces.ChartData;
using Application.Common.Abstractions.NamedObjects;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace Analytics.Application.NamedObjects.ChartDataSlices;

public abstract class ChartDataSourceNamedObject : LocalizableNameObject
{
    public abstract Type DataPointType { get; }
    public abstract Type QueryInputType { get; }

    public abstract Task<IReadOnlyList<IChartDataPoint>> QueryAsync(
        IChartQueryInput queryInput,
        CancellationToken cancellationToken);
}

public abstract class ChartDataSourceNamedObject<TDataPoint, TQueryInput>(
    ISchemaGenerator schemaGenerator) :
    ChartDataSourceNamedObject
    where TDataPoint : class, IChartDataPoint
    where TQueryInput : IChartQueryInput
{
    public sealed override Type DataPointType => typeof(TDataPoint);
    public sealed override Type QueryInputType => typeof(TQueryInput);

    public ObjectSchema DataPointSchema => schemaGenerator.Generate<TDataPoint>();
    public ObjectSchema QueryInputSchema => schemaGenerator.Generate<TQueryInput>();
    
    public abstract Task<IReadOnlyList<TDataPoint>> QueryAsync(
        TQueryInput queryInput,
        CancellationToken cancellationToken);

    public sealed override async Task<IReadOnlyList<IChartDataPoint>> QueryAsync(
        IChartQueryInput queryInput,
        CancellationToken cancellationToken)
    {
        if (queryInput is not TQueryInput typedInput)
            throw new ArgumentException(
                $"Expected query input of type '{typeof(TQueryInput).Name}', " +
                $"but received '{queryInput.GetType().Name}'.",
                nameof(queryInput));
        

        return await QueryAsync(typedInput, cancellationToken);
    }
}
