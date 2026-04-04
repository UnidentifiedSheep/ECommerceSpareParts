using Abstractions.Interfaces.HostedServices;
using Carter;
using MediatR;
using Search.Application.Handler.Articles.IsSuggestionsRebuilding;
using Search.Application.Handler.Articles.RebuildSuggestions;

namespace Search.Api.EndPoints.Suggestions;

public class RebuildSuggestionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/suggestions/rebuild", async (
            IBackgroundTaskQueue taskQueue,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new IsSuggestionsRebuildingQuery(true), cancellationToken);
            taskQueue.Enqueue(async ct => await mediator.Send(new RebuildSuggestionsCommand(), ct));
            return Results.Accepted();
        });
    }
}