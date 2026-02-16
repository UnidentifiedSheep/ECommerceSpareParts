using Abstractions.Interfaces.Services;
using Analytics.Core.Interfaces.Services;
using Attributes;
using Contracts.Models.Sale;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleEditedConsumer(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IConsumer<SaleEditedEvent>
{
    public async Task Consume(ConsumeContext<SaleEditedEvent> context)
    {
        
    }
}