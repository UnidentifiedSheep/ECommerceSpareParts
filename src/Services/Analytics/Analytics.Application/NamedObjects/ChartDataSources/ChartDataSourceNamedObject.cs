using Analytics.Application.Interfaces.ChartData;
using Analytics.Application.Models;
using Application.Common.Abstractions.NamedObjects;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace Analytics.Application.NamedObjects.ChartDataSources;

public abstract class ChartDataSourceNamedObject : LocalizableNameObject
{
	public abstract Type DataPointType { get; }

	public abstract Type QueryInputType { get; }

	public abstract ObjectSchema DataPointSchema { get; }

	public abstract ObjectSchema QueryInputSchema { get; }

	public abstract Task<ChartDataResult> QueryAsync(
		IChartQueryInput queryInput,
		CancellationToken cancellationToken);
}

public abstract class ChartDataSourceNamedObject<TDataPoint, TQueryInput>(ISchemaGenerator schemaGenerator)
	: ChartDataSourceNamedObject
	where TDataPoint : class, IChartDataPoint where TQueryInput : IChartQueryInput
{
	public sealed override Type DataPointType => typeof(TDataPoint);

	public sealed override Type QueryInputType => typeof(TQueryInput);

	public override ObjectSchema DataPointSchema => schemaGenerator.Generate<TDataPoint>();

	public override ObjectSchema QueryInputSchema => schemaGenerator.Generate<TQueryInput>();

	public abstract Task<ChartDataResult<TDataPoint>> QueryAsync(
		TQueryInput queryInput,
		CancellationToken cancellationToken);

	public sealed override async Task<ChartDataResult> QueryAsync(
		IChartQueryInput queryInput,
		CancellationToken cancellationToken)
	{
		if (queryInput is not TQueryInput typedInput)
			throw new ArgumentException(
				$"Expected query input of type '{typeof(TQueryInput).Name}', " +
				$"but received '{queryInput.GetType().Name}'.",
				nameof(queryInput));

		return (await QueryAsync(typedInput, cancellationToken)).ToUntyped();
	}
}
