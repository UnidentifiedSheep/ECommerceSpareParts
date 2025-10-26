using Analytics.Core.Interfaces.Services;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class SaleDeletedEventHandler(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IEventHandler<SaleDeletedEvent>
{
    public async Task HandleAsync(IEventContext<SaleDeletedEvent> context)
    {
        var deletedIds = context.Message.Sale.SaleContents.Select(s => s.Id);
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            await sellInfoService.RemoveSellInfos(deletedIds);
            await unitOfWork.SaveChangesAsync();
        });
    }
}