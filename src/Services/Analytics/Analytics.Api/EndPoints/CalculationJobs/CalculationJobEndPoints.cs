using Abstractions.Interfaces;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;
using Analytics.Application.Handlers.CalculationJob.GetCalculationJob;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.CalculationJobs;

public record CreateCalculationJobRequest(string MetricSystemName, MetricPayloadDto MetricPayload);

public record CreateCalculationJobResponse(Guid RequestId);

public record GetCalculationJobResponse(CalculationJobDto CalculationJob);

public class CalculationJobEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var jobs = app
            .MapGroup("/jobs")
            .WithTags("CalculationJobs");
            
        jobs.MapPost("", async (
                ISender sender,
                CreateCalculationJobRequest request,
                IUserContext userContext,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new CreateCalculationJobCommand(
                    request.MetricSystemName,
                    request.MetricPayload,
                    userContext.UserId), ct);

                return Results.Created(
                    $"/jobs/{result.CalculationJob.RequestId}",
                    new CreateCalculationJobResponse(result.CalculationJob.RequestId));
            }).WithName("CreateCalculationJob")
            .WithDisplayName("Create calculation job")
            .Produces<CreateCalculationJobResponse>(StatusCodes.Status201Created)
            .Produces(400);
        
        
        jobs.MapGet("{id}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCalculationJobQuery(id), cancellationToken);
                return Results.Ok(new GetCalculationJobResponse(result.CalculationJob));
            }).WithName("GetCalculationJob")
            .WithDescription("Gets a calculation job by id.")
            .Produces<GetCalculationJobResponse>()
            .Produces(404);
    }
}