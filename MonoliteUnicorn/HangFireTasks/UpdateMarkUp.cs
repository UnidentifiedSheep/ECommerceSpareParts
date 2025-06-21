using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.HangFireTasks;

public class UpdateMarkUp(MarkUpGenerator markUpGenerator)
{
    public async Task Run()
    {
        await markUpGenerator.ReCalculateMarkupAsync();
    }
}