using Abstractions.Interfaces;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.CalculationJobs;

public record CreateCalculationJobRequest(string MetricSystemName, MetricPayloadDto MetricPayload);
public record CreateCalculationJobResponse(Guid RequestId);

public class CreateCalculationJobEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("jobs/", async (
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
    }
}