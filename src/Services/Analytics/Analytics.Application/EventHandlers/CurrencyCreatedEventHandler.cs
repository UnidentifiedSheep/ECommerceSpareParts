using Contracts.Currency;
using Contracts.Models.Currency;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;
using Core.Models;
using Mapster;

namespace Analytics.Application.EventHandlers;

public class CurrencyCreatedEventHandler(IUnitOfWork unitOfWork) : IEventHandler<CurrencyCreatedEvent>
{
    public async Task HandleAsync(IEventContext<CurrencyCreatedEvent> context)
    {
        Currency model = context.Message.Currency.Adapt<Currency>();
        await unitOfWork.AddAsync(model);
        await unitOfWork.SaveChangesAsync();
    }
}