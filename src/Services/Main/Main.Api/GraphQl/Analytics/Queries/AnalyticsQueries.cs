using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Analytics.DataLoaders;
using Main.Api.GraphQl.Analytics.Types;
using Main.Application.Analytics.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.Queries;

public sealed class AnalyticsQueries
{
	[GraphQLName("charts")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<IReadOnlyList<GqlChart>> GetChartsAsync(ISender sender, CancellationToken cancellationToken) =>
		(await sender.Send(new GetChartsQuery(), cancellationToken)).Charts.Select(chart => new GqlChart(chart)).ToArray();

	[GraphQLName("chart")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<GqlChart?> GetChartAsync(string systemName, IChartBySystemNameDataLoader loader,
		CancellationToken cancellationToken)
	{
		var chart = await loader.LoadAsync(systemName, cancellationToken);
		return chart is null ? null : new GqlChart(chart);
	}
}
