using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Analytics.Inputs;

[GraphQLName("GetAnalyticsRunsInput")]
public sealed record GqlGetAnalyticsRunsInput
{
	[GraphQLName("cursor")]
	public required GqlCursor<GqlAnalyticsRunCursorValueInput?> Cursor { get; init; }

	[GraphQLName("analysisType")]
	public string? AnalysisType { get; init; }

	[GraphQLName("isPinned")]
	public bool? IsPinned { get; init; }

	[GraphQLName("periodFrom")]
	public DateTime? PeriodFrom { get; init; }

	[GraphQLName("periodTo")]
	public DateTime? PeriodTo { get; init; }
}
