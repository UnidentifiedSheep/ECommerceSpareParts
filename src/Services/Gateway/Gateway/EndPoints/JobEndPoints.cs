using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Gateway.Application.Dtos;
using Gateway.Application.Handlers;
using Localization.Abstractions.Interfaces;
using MediatR;

namespace Gateway.EndPoints;

public record GetAggregatedAvailableJobsResponse
{
    [JsonPropertyName("jobs")]
    public required ServiceJobsDto[] Jobs { get; init; }
}

public static class JobEndPoints
{
    public static IEndpointRouteBuilder MapJobEndPoints(this IEndpointRouteBuilder app)
    {
        var jobs = app.MapGroup("/jobs")
            .WithTags("Jobs");

        jobs.MapGet(
                "/available",
                async (
                    ISender sender,
                    IScopedStringLocalizer localizer,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetAggregatedAvailableJobsQuery(localizer.Locale), ct);
                    return Results.Ok(
                        new GetAggregatedAvailableJobsResponse
                        {
                            Jobs = result.Jobs
                        });
                })
            .WithName("GetAvailableGatewayJobs")
            .WithDisplayName("Get available jobs from all services")
            .Produces<GetAggregatedAvailableJobsResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        return app;
    }
}