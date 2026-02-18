using Abstractions.Interfaces.HostedServices;
using Contracts.Search;
using MassTransit;
using Mediator;
using Search.Application.Handler.Articles.IsSuggestionsRebuilding;
using Search.Application.Handler.Articles.RebuildSuggestions;

namespace Search.Application.Consumers;

public class SuggestionRebuildNeededConsumer(IBackgroundTaskQueue taskQueue, IMediator mediator) : IConsumer<SuggestionRebuildNeededEvent>
{
    public async Task Consume(ConsumeContext<SuggestionRebuildNeededEvent> context)
    {
        var result = await mediator.Send(new IsSuggestionsRebuildingQuery(false));
        if (result.IsRebuilding) return;
        taskQueue.Enqueue(async ct => await mediator.Send(new RebuildSuggestionsCommand(), ct));
    }
}