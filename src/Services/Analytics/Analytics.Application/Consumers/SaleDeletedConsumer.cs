using Abstractions.Interfaces.Services;
using Analytics.Core.Interfaces.Services;
using Attributes;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleDeletedConsumer(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IConsumer<SaleDeletedEvent>
{
    public async Task Consume(ConsumeContext<SaleDeletedEvent> context)
    {
        var deletedIds = context.Message.Sale.SaleContents.Select(s => s.Id);
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            await sellInfoService.RemoveSellInfos(deletedIds);
            await unitOfWork.SaveChangesAsync();
        });
    }
}