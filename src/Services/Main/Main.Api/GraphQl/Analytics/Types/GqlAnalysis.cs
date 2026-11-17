using GraphQL.Common.Types.Schema;
using HotChocolate;
using Main.Modules.Analytics.Application.Dtos.Analyses;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("Analysis")]
public sealed record GqlAnalysis([property: GraphQLIgnore]
	AnalysisDto Analysis)
{
	[GraphQLName("systemName")]
	public string SystemName => Analysis.SystemName;

	[GraphQLName("name")]
	public string Name => Analysis.Name;

	[GraphQLName("description")]
	public string Description => Analysis.Description;

	[GraphQLName("lrtSystemName")]
	public string LrtSystemName => Analysis.LrtSystemName;

	[GraphQLName("itemsChartSystemName")]
	public string ItemsChartSystemName => Analysis.ItemsChartSystemName;

	[GraphQLName("schemaVersion")]
	public int SchemaVersion => Analysis.SchemaVersion;

	[GraphQLName("parametersSchema")]
	public GqlObjectSchema ParametersSchema => new(Analysis.ParametersSchema);

	[GraphQLName("summarySchema")]
	public GqlObjectSchema SummarySchema => new(Analysis.SummarySchema);

	[GraphQLName("segmentsSchema")]
	public GqlObjectSchema SegmentsSchema => new(Analysis.SegmentsSchema);

	[GraphQLName("dataSchema")]
	public GqlObjectSchema DataSchema => new(Analysis.DataSchema);
}
