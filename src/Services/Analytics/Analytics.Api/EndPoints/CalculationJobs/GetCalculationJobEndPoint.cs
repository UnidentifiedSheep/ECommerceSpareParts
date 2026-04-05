using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Application.Handlers.CalculationJob.GetCalculationJob;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.CalculationJobs;

public record GetCalculationJobResponse(CalculationJobDto CalculationJob);

public class GetCalculationJobEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("jobs/{id}", async (
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