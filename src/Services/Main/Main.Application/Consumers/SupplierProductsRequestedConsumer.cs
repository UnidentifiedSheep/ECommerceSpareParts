using Contracts.Supplier;
using MassTransit;

namespace Main.Application.Consumers;

public class SupplierProductsRequestedConsumer : IConsumer<SupplierProductsRequestedEvent>
{
    public Task Consume(ConsumeContext<SupplierProductsRequestedEvent> context)
    {
        throw new NotImplementedException();
    }
}