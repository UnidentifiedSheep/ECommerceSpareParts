using Analytics.Application.Dtos.Metric;
using Analytics.Application.Handlers.Metrics.GetMetrics;
using Analytics.Application.Handlers.Metrics.ListMetrics;
using Api.Common.Models.Requests;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.Metrics;

public sealed record GetMetricsRequest() : SortablePaginationQueryModel;
public sealed record GetMetricsResponse(IReadOnlyList<MetricDto> Metrics);

public sealed record GetMetricInfosResponse(IReadOnlyList<MetricInfoDto> Metrics);

public class MetricEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var metrics = app.MapGroup("/metrics")
            .WithTags("Metrics");
        
        metrics.MapGet("/info", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new ListAvailableMetricsQuery(), ct);
                return Results.Ok(new GetMetricInfosResponse(result.Metrics));
            }).WithName("GetMetricInfos")
            .WithDescription("Выводит все существующие метрики")
            .Produces<GetMetricInfosResponse>();
        
        metrics.MapGet("", async (
            ISender sender, 
            [AsParameters] GetMetricsRequest request, 
            CancellationToken token) =>
        {
            var result = await sender.Send(new GetMetricsQuery(
                request.SortBy,
                request), token);
            
            return Results.Ok(new GetMetricsResponse(result.Metrics));
        }).WithName("GetMetrics")
        .WithDescription("Выводит метрики")
        .Produces<GetMetricsResponse>();
    }
}