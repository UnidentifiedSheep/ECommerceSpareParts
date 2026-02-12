using Abstractions.Interfaces.Services;
using Analytics.Core.Entities;
using Contracts.Currency;
using Mapster;
using MassTransit;

namespace Analytics.Application.Consumers;

public class CurrencyCreatedConsumer(IUnitOfWork unitOfWork) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        Currency model = context.Message.Currency.Adapt<Currency>();
        await unitOfWork.AddAsync(model);
        await unitOfWork.SaveChangesAsync();
    }
}