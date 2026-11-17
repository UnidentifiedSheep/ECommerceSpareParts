using NamedObject.Core.Base;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace Main.Modules.Analytics.Abstractions.Analyses;

public abstract class AnalysisNamedObject : LocalizableNameObject
{
	public abstract string LrtSystemName { get; }

	public abstract string ItemsChartSystemName { get; }

	public abstract int SchemaVersion { get; }

	public abstract ObjectSchema ParametersSchema { get; }

	public abstract ObjectSchema SummarySchema { get; }

	public abstract ObjectSchema SegmentsSchema { get; }

	public abstract ObjectSchema DataSchema { get; }
}

public abstract class AnalysisNamedObject<TParameters, TSummary, TSegments, TData>(ISchemaGenerator schemaGenerator)
	: AnalysisNamedObject
{
	public sealed override ObjectSchema ParametersSchema => schemaGenerator.Generate<TParameters>();

	public sealed override ObjectSchema SummarySchema => schemaGenerator.Generate<TSummary>();

	public sealed override ObjectSchema SegmentsSchema => schemaGenerator.Generate<TSegments>();

	public sealed override ObjectSchema DataSchema => schemaGenerator.Generate<TData>();
}
