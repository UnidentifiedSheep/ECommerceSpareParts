using System.Text.Json;
using System.Text.Json.Serialization;
using Analytics.Application.Dtos.Charts;
using Analytics.Application.Handlers.ChartDataSources;
using Api.Common.Extensions;
using Carter;
using Enums;
using MediatR;

namespace Analytics.Api.EndPoints;

public sealed record GetChartsResponse
{
	[JsonPropertyName("charts")]
	public required IReadOnlyList<ChartDto> Charts { get; init; }
}

public sealed record QueryChartDataSourceRequest
{
	[JsonPropertyName("queryInput")]
	public required JsonElement QueryInput { get; init; }
}

public sealed record QueryChartDataSourceResponse
{
	[JsonPropertyName("dataPoints")]
	public required IReadOnlyList<object> DataPoints { get; init; }

	[JsonPropertyName("nextCursor")]
	public string? NextCursor { get; init; }
}

public sealed class ChartEndPoints : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app
			.MapGet(
				"/charts",
				async (ISender sender, CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(new GetChartsQuery(), cancellationToken);

					return Results.Ok(
						new GetChartsResponse
						{
							Charts = result.Charts
						});
				})
			.WithTags("Charts")
			.WithName("GetCharts")
			.WithSummary("Получение доступных графиков")
			.WithDescription("Возвращает доступные графики и схемы их входных параметров и точек данных")
			.WithDisplayName("Получение доступных графиков")
			.Produces<GetChartsResponse>()
			.RequireAllPermissions(PermissionCodes.CHARTS_GET);

		app
			.MapPost(
				"/charts/{systemName}/query",
				async (
					ISender sender, string systemName,
					QueryChartDataSourceRequest request, CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new QueryChartDataSourceQuery(systemName, request.QueryInput.GetRawText()),
						cancellationToken);

					return Results.Ok(
						new QueryChartDataSourceResponse
						{
							DataPoints = result.DataPoints, NextCursor = result.NextCursor
						});
				})
			.WithTags("Charts")
			.WithName("QueryChartDataSource")
			.WithSummary("Получение данных графика")
			.WithDescription("Выполняет запрос к выбранному источнику данных графика")
			.WithDisplayName("Получение данных графика")
			.Accepts<QueryChartDataSourceRequest>(false, "application/json")
			.Produces<QueryChartDataSourceResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.RequireAllPermissions(PermissionCodes.CHARTS_GET);
	}
}
