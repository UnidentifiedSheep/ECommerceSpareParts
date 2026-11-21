using Domain.CommonEntities.Job;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Modules.Jobs.Api.GraphQl.Types.Inputs;

[GraphQLName("SearchJobSchedulesInput")]
public sealed record GqlSearchJobSchedulesInput
{
	[GraphQLName("jobSystemNames")]
	public IReadOnlyCollection<string> JobSystemNames { get; init; } = [];

	[GraphQLName("nextRunFrom")]
	public DateTime? NextRunFrom { get; init; }

	[GraphQLName("nextRunTo")]
	public DateTime? NextRunTo { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("sortBy")]
	[SortableBy<JobSchedule>]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
