using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Core.Interfaces.Services;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class SaleEditedEventHandler(IUnitOfWork unitOfWork, ISellInfoRepository sellInfoRepository, 
    ISellInfoService sellInfoService) : IEventHandler<SaleEditedEvent>
{
    public async Task HandleAsync(IEventContext<SaleEditedEvent> context)
    {
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            await sellInfoService.RemoveSellInfos(context.Message.DeletedSaleContents);
        });
    }
}