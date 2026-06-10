using Api.Common.Extensions;
using Enums;
using Gateway.Services.Jobs;

namespace Gateway.EndPoints;

public static class JobEndPoints
{
    public static IEndpointRouteBuilder MapJobEndPoints(this IEndpointRouteBuilder app)
    {
        var jobs = app.MapGroup("/jobs")
            .WithTags("Jobs");

        jobs.MapGet("", async (
                IJobAggregationService aggregationService,
                HttpContext context,
                CancellationToken ct) =>
            {
                var result = await aggregationService.GetJobsAsync(context, ct);
                return Results.Ok(result);
            })
            .WithName("GetGatewayJobs")
            .WithDisplayName("Get jobs from all services")
            .Produces<GetGatewayJobsResponse>();

        return app;
    }
}
