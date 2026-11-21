using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Modules.Jobs.Api.GraphQl.Types.Inputs;

[GraphQLName("SearchJobsInput")]
public sealed record GqlSearchJobsInput
{
	[GraphQLName("systemNames")]
	public IReadOnlyCollection<string> SystemNames { get; init; } = [];

	[GraphQLName("statuses")]
	public IReadOnlyCollection<JobStatus> Statuses { get; init; } = [];

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("sortBy")]
	[SortableBy<Job>]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
