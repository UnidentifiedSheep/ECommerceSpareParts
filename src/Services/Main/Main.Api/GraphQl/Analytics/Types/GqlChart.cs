using System.Text.Json;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types.Schema;
using HotChocolate;
using HotChocolate.Types;
using Main.Modules.Analytics.Application.Dtos.Charts;
using Main.Modules.Analytics.Application.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("Chart")]
public sealed record GqlChart([property: GraphQLIgnore]
	ChartDto Chart)
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
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new QueryChartDataSourceQuery(SystemName, queryInput.GetRawText()),
			cancellationToken);
		var points = result.DataPoints
			.Select(point => new GqlChartDataPoint(
				JsonSerializer.SerializeToElement(
					point,
					point.GetType(),
					JsonSerializerOptions.Web),
				point.GetReferences()))
			.ToArray();
		return new GqlChartData
		{
			DataPoints = points.Select(point => point.Data).ToArray(),
			Points = points,
			NextCursor = result.NextCursor
		};
	}
}
