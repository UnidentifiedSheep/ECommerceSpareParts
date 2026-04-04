using Application.Common.Interfaces;
using Search.Abstractions.Exceptions.Suggestions;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Handler.Articles.IsSuggestionsRebuilding;

public record IsSuggestionsRebuildingQuery(bool ThrowOnRebuilding) : IQuery<IsSuggestionsRebuildingResult>;

public record IsSuggestionsRebuildingResult(bool IsRebuilding);

public class IsSuggestionsRebuildingHandler(IArticleSuggestionService suggestionService)
    : IQueryHandler<IsSuggestionsRebuildingQuery, IsSuggestionsRebuildingResult>
{
    public Task<IsSuggestionsRebuildingResult> Handle(
        IsSuggestionsRebuildingQuery query,
        CancellationToken cancellationToken)
    {
        var rebuilding = suggestionService.IsRebuildingNow();
        if (rebuilding && query.ThrowOnRebuilding) throw new SuggestionsRebuildingException();
        return Task.FromResult(new IsSuggestionsRebuildingResult(rebuilding));
    }
}