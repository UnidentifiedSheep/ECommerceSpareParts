using Core.RabbitMq.Contracts;
using MassTransit;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.Consumers;

public class MarkupGroupChangedConsumer(DContext dContext) : IConsumer<MarkupGroupChangedEvent>
{
    public async Task Consume(ConsumeContext<MarkupGroupChangedEvent> context)
    {
        SetupPriceGenerator.Settings.SelectedMarkupId = context.Message.GroupId;
        await SetupPriceGenerator.SetMarkupsAsync(dContext);
    }
}
