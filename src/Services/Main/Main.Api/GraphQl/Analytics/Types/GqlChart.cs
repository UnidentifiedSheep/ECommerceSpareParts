using System.Text.Json;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types.Schema;
using HotChocolate;
using HotChocolate.Types;
using Main.Application.Analytics.Dtos.Charts;
using Main.Application.Analytics.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("Chart")]
public sealed record GqlChart([property: GraphQLIgnore] ChartDto Chart)
{
	[GraphQLName("systemName")]
	public string SystemName => Chart.SystemName;

	[GraphQLName("name")]
	public string Name => Chart.Name;

	[GraphQLName("description")]
	public string Description => Chart.Description;

	[GraphQLName("queryInputSchema")]
	public GqlObjectSchema QueryInputSchema => new(Chart.QueryInputSchema);

	[GraphQLName("dataPointSchema")]
	public GqlObjectSchema DataPointSchema => new(Chart.DataPointSchema);

	[GraphQLName("data")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<GqlChartData> GetDataAsync(
		[GraphQLType<NonNullType<AnyType>>] JsonElement queryInput,
		ISender sender, CancellationToken cancellationToken)
	{
		var result = await sender.Send(new QueryChartDataSourceQuery(SystemName, queryInput.GetRawText()), cancellationToken);
		return new GqlChartData
		{
			DataPoints = result.DataPoints
				.Select(point => JsonSerializer.SerializeToElement(point, point.GetType(), JsonSerializerOptions.Web)).ToArray(),
			NextCursor = result.NextCursor
		};
	}
}
