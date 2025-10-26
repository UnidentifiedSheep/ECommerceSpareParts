using Analytics.Core.Interfaces.Services;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class SaleCreatedEventHandler(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IEventHandler<SaleCreatedEvent>
{
    public async Task HandleAsync(IEventContext<SaleCreatedEvent> context)
    {
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            await sellInfoService.CreateSellInfos(context.Message.Sale);
            await unitOfWork.SaveChangesAsync();
        });
    }
}