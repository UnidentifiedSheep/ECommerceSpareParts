using Abstractions.Interfaces.HostedServices;
using Mediator;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.GetSuggestions;
using Search.Application.Handler.Articles.IsSuggestionsRebuilding;
using Search.Application.Handler.Articles.RebuildSuggestions;

namespace Search.Api.EndPoints;

public record GetSuggestionsResponse(List<ArticleDto> Suggestions);

public static class SuggestionEndPoints
{
    public static IEndpointRouteBuilder MapSuggestionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/suggestions");

        group.MapGet("/", GetAll);
        group.MapPost("/rebuild", Rebuild);

        return routes;
    }

    static async Task<IResult> GetAll(string query, int limit, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSuggestionsQuery(query, limit), cancellationToken);
        return Results.Ok(new GetSuggestionsResponse(result.Suggestions));
    }

    static async Task<IResult> Rebuild(IBackgroundTaskQueue taskQueue, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new IsSuggestionsRebuildingQuery(true), cancellationToken);
        taskQueue.Enqueue(async ct => await mediator.Send(new RebuildSuggestionsCommand(), ct));
        return Results.Accepted();
    }
}