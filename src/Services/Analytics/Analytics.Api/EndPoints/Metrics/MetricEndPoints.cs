using Analytics.Application.Dtos.Metric;
using Analytics.Application.Handlers.Metrics;
using Analytics.Application.Handlers.Metrics.GetMetrics;
using Analytics.Application.Handlers.Metrics.ListAvailableMetrics;
using Api.Common.Models.Requests;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.EndPoints.Metrics;

public sealed record GetMetricJobsRequest() : SortablePaginationQueryModel;

public sealed record GetMetricsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "metricSystemName")]
    public string? MetricSystemName { get; init; }
}
public sealed record GetMetricsResponse(IReadOnlyList<MetricDto> Metrics);

public sealed record GetMetricInfosResponse(IReadOnlyList<MetricInfoDto> Metrics);
public sealed record GetMetricJobsResponse(IReadOnlyList<MetricJobDto> Jobs);

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
                request.MetricSystemName,
                request.SortBy,
                request), token);
            
            return Results.Ok(new GetMetricsResponse(result.Metrics));
        }).WithName("GetMetrics")
        .WithDescription("Выводит метрики")
        .Produces<GetMetricsResponse>();

        metrics.MapGet("{metricId:guid}/jobs", async (
            ISender sender,
            Guid metricId,
            [AsParameters] GetMetricJobsRequest request,
            CancellationToken ct) =>
        {
            var query = new GetMetricJobsQuery(
                metricId,
                request,
                request.SortBy);
            
            var result = await sender.Send(query, ct);
            return Results.Ok(new GetMetricJobsResponse(result.Jobs));
        }).WithName("GetMetricJobs")
        .WithDescription("Выводит историю расчетов метрики.")
        .Produces<GetMetricJobsResponse>();
    }
}
