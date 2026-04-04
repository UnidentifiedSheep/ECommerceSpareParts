using Carter;
using MediatR;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.GetSuggestions;

namespace Search.Api.EndPoints.Suggestions;

public record GetSuggestionsResponse(List<ArticleDto> Suggestions);

public class GetSuggestionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/suggestions", async (
            string query,
            int limit,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSuggestionsQuery(query, limit), cancellationToken);
            return Results.Ok(new GetSuggestionsResponse(result.Suggestions));
        }).WithName("GetArticleSuggestions")
        .WithDescription("Gets suggestions by query")
        .Produces<GetSuggestionsResponse>();
    }
}