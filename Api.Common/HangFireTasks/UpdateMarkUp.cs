using Core.Interfaces;

namespace Api.Common.HangFireTasks;

public class UpdateMarkUp(IServiceProvider serviceProvider)
{
    public async Task Run()
    {
        using var scope = serviceProvider.CreateScope();
        var markupGenerator = scope.ServiceProvider.GetRequiredService<IMarkupGenerator>();
        await markupGenerator.ReCalculateMarkupAsync();
    }
}