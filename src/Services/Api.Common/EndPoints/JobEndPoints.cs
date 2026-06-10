using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Application.Common.Dtos;
using Application.Common.Handlers.Jobs;
using Carter;
using Enums;
using MediatR;

namespace Api.Common.EndPoints;

public record GetJobsResponse
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<JobInfoDto> Jobs { get; init; }
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

public class JobEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var jobs = app.MapGroup("/jobs")
            .WithTags("Jobs");

        jobs.MapGet("", async (
            ISender sender, 
            CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAllAvailableJobsQuery(), ct);
                
                return Results.Ok(new GetJobsResponse
                {
                    Jobs = result.Jobs
                });
            }).WithName("GetJobs")
        .WithDisplayName("Get all jobs")
        .Produces<GetJobsResponse>()
        .RequireAllPermissions(PermissionCodes.JOBS_GET);
        
        jobs.MapPost("", async (
                ISender sender, 
                CreateJobRequest request,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new QueueJobCommand(
                    request.SystemName,
                    request.InputState,
                    request.MaxAttempts), ct);
                
                return Results.Created(
                    $"/jobs/{result.Job.Id}", 
                    new CreateJobResponse
                    {
                        Job = result.Job
                    });
            }).WithName("CreateJob")
            .WithDisplayName("Creates a new job")
            .Produces<CreateJobResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_CREATE);
    }
}
