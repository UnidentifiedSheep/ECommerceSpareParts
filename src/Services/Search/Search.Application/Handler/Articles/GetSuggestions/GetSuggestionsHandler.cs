using Mediator;
using Search.Abstractions.Dtos;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;
using Search.Entities;

namespace Search.Application.Handler.Articles.GetSuggestions;

public record GetSuggestionsQuery(string Query, int Limit) : IQuery<GetSuggestionsResult>;
public record GetSuggestionsResult(List<ArticleDto> Suggestions);

public class GetSuggestionsHandler(IArticleSuggestionService suggestionService) : IQueryHandler<GetSuggestionsQuery, GetSuggestionsResult>
{
    public ValueTask<GetSuggestionsResult> Handle(GetSuggestionsQuery query, CancellationToken cancellationToken)
    {
        List<Article> suggestions = suggestionService.GetSuggestions(query.Query, query.Limit);
        List<ArticleDto> adaptedSuggestions = suggestions.ToDtos();
        return ValueTask.FromResult(new GetSuggestionsResult(adaptedSuggestions));
    }
}