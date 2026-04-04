using Application.Common.Interfaces;
using Search.Abstractions.Dtos;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;

namespace Search.Application.Handler.Articles.GetSuggestions;

public record GetSuggestionsQuery(string Query, int Limit) : IQuery<GetSuggestionsResult>;

public record GetSuggestionsResult(List<ArticleDto> Suggestions);

public class GetSuggestionsHandler(IArticleSuggestionService suggestionService)
    : IQueryHandler<GetSuggestionsQuery, GetSuggestionsResult>
{
    public Task<GetSuggestionsResult> Handle(GetSuggestionsQuery query, CancellationToken cancellationToken)
    {
        var suggestions = suggestionService.GetSuggestions(query.Query, query.Limit);
        var adaptedSuggestions = suggestions.ToDtos();
        return Task.FromResult(new GetSuggestionsResult(adaptedSuggestions));
    }
}