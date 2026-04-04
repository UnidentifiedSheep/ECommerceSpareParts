using Application.Common.Interfaces;
using MediatR;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Handler.Articles.RebuildSuggestions;

public record RebuildSuggestionsCommand : ICommand;

public class RebuildSuggestionsHandler(IArticleSuggestionService suggestionService)
    : ICommandHandler<RebuildSuggestionsCommand>
{
    public async Task<Unit> Handle(RebuildSuggestionsCommand command, CancellationToken cancellationToken)
    {
        await suggestionService.RebuildSuggestions();
        return Unit.Value;
    }
}