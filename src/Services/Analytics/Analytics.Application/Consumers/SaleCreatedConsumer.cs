using Abstractions.Interfaces.Services;
using Analytics.Core.Interfaces.Services;
using Attributes;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleCreatedConsumer(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IConsumer<SaleCreatedEvent>
{
    public async Task Consume(ConsumeContext<SaleCreatedEvent> context)
    {
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
        });
    }
}