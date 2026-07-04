using System.Text.Json.Serialization;
using Analytics.Application.Dtos.Metric;
using Analytics.Application.Handlers.Metrics;
using Analytics.Application.Handlers.Metrics.GetMetrics;
using Analytics.Application.Handlers.Metrics.ListAvailableMetrics;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.EndPoints;

public sealed record GetMetricJobsRequest : SortablePaginationQueryModel;

public sealed record GetMetricsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "metricSystemName")]
    public string? MetricSystemName { get; init; }
}

public sealed record GetMetricsResponse
{
    [JsonPropertyName("metrics")]
    public required IReadOnlyList<MetricDto> Metrics { get; init; }
}

public sealed record GetMetricInfosResponse
{
    [JsonPropertyName("metrics")]
    public required IReadOnlyList<MetricInfoDto> Metrics { get; init; }
}

public sealed record UpsertMetricRequest
{
    [JsonPropertyName("metricSystemName")]
    public required string MetricSystemName { get; init; }

    [JsonPropertyName("inputPayload")]
    public required string InputPayload { get; init; }
}

public sealed record UpsertMetricResponse
{
    [JsonPropertyName("metric")]
    public required MetricDto Metric { get; init; }
}

public sealed record GetMetricJobsResponse
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<MetricJobDto> Jobs { get; init; }
}

public class MetricEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var metrics = app.MapGroup("/metrics")
            .WithTags("Metrics");

        metrics.MapGet(
                "/info",
                async (ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new ListAvailableMetricsQuery(), ct);
                    return Results.Ok(
                        new GetMetricInfosResponse
                        {
                            Metrics = result.Metrics
                        });
                })
            .WithName("GetMetricInfos")
            .WithDescription("Выводит все существующие метрики")
            .Produces<GetMetricInfosResponse>()
            .RequireAnyPermission(PermissionCodes.METRICS_GET);

        metrics.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetMetricsRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new GetMetricsQuery(
                            request.MetricSystemName,
                            request.SortBy,
                            request),
                        token);

                    return Results.Ok(
                        new GetMetricsResponse
                        {
                            Metrics = result.Metrics
                        });
                })
            .WithName("GetMetrics")
            .WithDescription("Выводит метрики")
            .Produces<GetMetricsResponse>()
            .RequireAnyPermission(PermissionCodes.METRICS_GET);

        metrics.MapPost(
                "",
                async (
                    ISender sender,
                    UpsertMetricRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new UpsertMetricCommand(
                            request.MetricSystemName,
                            request.InputPayload),
                        token);

                    return Results.Ok(
                        new UpsertMetricResponse
                        {
                            Metric = result.Metric
                        });
                })
            .WithName("UpsertMetric")
            .WithDescription("Создает метрику или возвращает существующую.")
            .Produces<UpsertMetricResponse>()
            .RequireAnyPermission(PermissionCodes.METRICS_CREATE);

        metrics.MapGet(
                "{metricId:guid}/jobs",
                async (
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
                    return Results.Ok(
                        new GetMetricJobsResponse
                        {
                            Jobs = result.Jobs
                        });
                })
            .WithName("GetMetricJobs")
            .WithDescription("Выводит историю расчетов метрики.")
            .Produces<GetMetricJobsResponse>()
            .RequireAnyPermission(PermissionCodes.METRICS_GET);
    }
}