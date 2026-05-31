using Abstractions.Interfaces;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;
using Analytics.Application.Handlers.CalculationJob.GetCalculationJob;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.CalculationJobs;

public record CreateCalculationJobRequest
{
    public string? MetricSystemName { get; init; }
    public MetricPayloadDto? MetricPayload { get; init; }
    public Guid? MetricId { get; init; }
}

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
                CreateCalculationJobCommand command;
                if (request.MetricId != null)
                    command = new CreateCalculationJobCommand(request.MetricId.Value, userContext.UserId);
                else if (request.MetricPayload != null && request.MetricSystemName != null)
                    command = new CreateCalculationJobCommand(request.MetricSystemName, userContext.UserId, request.MetricPayload);
                else
                    throw new InvalidOperationException("Invalid request");
                
                var result = await sender.Send(command, ct);

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