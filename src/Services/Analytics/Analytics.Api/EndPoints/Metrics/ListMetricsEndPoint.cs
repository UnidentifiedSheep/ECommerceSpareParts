using Analytics.Abstractions.Dtos.Metric;
using Analytics.Application.Handlers.Metrics.ListMetrics;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.Metrics;

public sealed record ListMetricsResponse(IReadOnlyList<MetricInfoDto> Metrics);

public class ListMetricsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/metrics", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new ListMetricsQuery(), ct);

                return Results.Ok(new ListMetricsResponse(result.Metrics));
            }).WithName("ListMetrics")
            .WithDescription("Выводит все существующие метрики")
            .Produces<ListMetricsResponse>();
    }
}