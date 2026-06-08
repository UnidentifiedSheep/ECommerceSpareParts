using System.Text.Json.Serialization;
using Application.Common.Dtos;
using Application.Common.Handlers.Jobs;
using Carter;
using MediatR;

namespace Api.Common.EndPoints;

public record GetJobsResponse
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<JobDto> Jobs { get; init; }
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
        .Produces<GetJobsResponse>();
    }
}
