using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Application.Common.Dtos;
using Application.Common.Handlers.Jobs;
using Application.Common.Handlers.Jobs.GetJobs;
using Carter;
using Domain.CommonEnums;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.EndPoints;

public record GetAvailableJobsResponse
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<JobInfoDto> Jobs { get; init; }
}

public record GetJobsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "systemName")]
    public string[] SystemNames { get; init; } = [];

    [FromQuery(Name = "status")]
    public JobStatus[] Statuses { get; init; } = [];
}

public record GetJobsResponse
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<JobDto> Jobs { get; init; }
}

public record CreateJobRequest
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("inputState")]
    public required string InputState { get; init; }

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts { get; init; } = 3;
}

public record CreateJobResponse
{
    [JsonPropertyName("job")]
    public required JobDto Job { get; init; }
}

public record GetJobStateResponse
{
    [JsonPropertyName("state")]
    public required string State { get; init; }
}

public class JobEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var jobs = app.MapGroup("/jobs")
            .WithTags("Jobs")
            .AddScheduleEndPoints();

        jobs.MapGet(
                "/available",
                async (
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetAllAvailableJobsQuery(), ct);

                    return Results.Ok(
                        new GetAvailableJobsResponse
                        {
                            Jobs = result.Jobs
                        });
                })
            .WithName("GetAvailableJobs")
            .WithDisplayName("Get all available jobs")
            .Produces<GetAvailableJobsResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        jobs.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetJobsRequest request,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new GetJobsQuery(
                            request,
                            request.SystemNames,
                            request.Statuses,
                            request.SortBy),
                        ct);

                    return Results.Ok(
                        new GetJobsResponse
                        {
                            Jobs = result.Jobs
                        });
                })
            .WithName("GetJobs")
            .WithDisplayName("Get current jobs")
            .Produces<GetJobsResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        jobs.MapGet(
                "{id:guid}/state",
                async (
                    ISender sender,
                    Guid id,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetJobStateQuery(id), ct);

                    return Results.Ok(
                        new GetJobStateResponse
                        {
                            State = result.State
                        });
                })
            .WithName("GetJobState")
            .WithDisplayName("Get current jobs state")
            .Produces<GetJobStateResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        jobs.MapPost(
                "",
                async (
                    ISender sender,
                    CreateJobRequest request,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new QueueJobCommand(
                            request.SystemName,
                            request.InputState,
                            request.MaxAttempts),
                        ct);

                    return Results.Created(
                        $"/jobs/{result.Jobs[0].Id}",
                        new CreateJobResponse
                        {
                            Job = result.Jobs[0]
                        });
                })
            .WithName("CreateJob")
            .WithDisplayName("Creates a new job")
            .Produces<CreateJobResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_CREATE);
    }
}