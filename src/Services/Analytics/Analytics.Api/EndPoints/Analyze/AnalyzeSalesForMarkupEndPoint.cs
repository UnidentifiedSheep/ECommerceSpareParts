using Analytics.Application.Handlers.AnalyzeSalesForMarkup;
using Carter;
using MediatR;

namespace Analytics.Api.EndPoints.Analyze;

public class AnalyzeSalesForMarkupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/analyze/sales/markup", async (ISender sender, CancellationToken cancellation) =>
        {
            await sender.Send(new AnalyzeSalesForMarkupCommand(), cancellation);
            return Results.Ok();
        });
    }
}