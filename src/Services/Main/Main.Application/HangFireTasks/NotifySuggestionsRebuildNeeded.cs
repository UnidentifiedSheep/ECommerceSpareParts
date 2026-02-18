using Contracts.Search;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application.HangFireTasks;

public class NotifySuggestionsRebuildNeeded(IServiceProvider serviceProvider)
{
    public async Task Run()
    {
        using var scope = serviceProvider.CreateScope();
        var publishEndPoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndPoint.Publish(new SuggestionRebuildNeededEvent());
    }
}