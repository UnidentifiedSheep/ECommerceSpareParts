using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;
using Main.Modules.Analytics.Application.Dtos.Runs;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("AnalyticsRun")]
public sealed record GqlAnalyticsRun([property: GraphQLIgnore]
	AnalyticsRunDto Run)
{
	[GraphQLName("id")]
	public Guid Id => Run.Id;

	[GraphQLName("analysisType")]
	public string AnalysisType => Run.AnalysisType;

	[GraphQLName("schemaVersion")]
	public int SchemaVersion => Run.SchemaVersion;

	[GraphQLName("parameters")]
	[GraphQLType<NonNullType<AnyType>>]
	public JsonElement Parameters => JsonSerializer.Deserialize<JsonElement>(Run.Parameters);

	[GraphQLName("summary")]
	[GraphQLType<AnyType>]
	public JsonElement? Summary
		=> Run.Summary is null ? null : JsonSerializer.Deserialize<JsonElement>(Run.Summary);

	[GraphQLName("itemsCount")]
	public int ItemsCount => Run.ItemsCount;

	[GraphQLName("periodStart")]
	public DateTime? PeriodStart => Run.PeriodStart;

	[GraphQLName("periodEnd")]
	public DateTime? PeriodEnd => Run.PeriodEnd;

	[GraphQLName("jobId")]
	public Guid? JobId => Run.JobId;

	[GraphQLName("createdBy")]
	public Guid? CreatedBy => Run.CreatedBy;

	[GraphQLName("isPinned")]
	public bool IsPinned => Run.IsPinned;

	[GraphQLName("createdAt")]
	public DateTime CreatedAt => Run.CreatedAt;
}
