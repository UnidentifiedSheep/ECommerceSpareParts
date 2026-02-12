using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Currency;
using Main.Application.Notifications;
using Main.Entities;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

[Transactional]
public record CreateCurrencyCommand(string ShortName, string Name, string CurrencySign, string Code)
    : ICommand<CreateCurrencyResult>;

public record CreateCurrencyResult(int Id);

public class CreateCurrencyHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IMediator mediator)
    : ICommandHandler<CreateCurrencyCommand, CreateCurrencyResult>
{
    public async Task<CreateCurrencyResult> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var model = request.Adapt<Currency>();
        await unitOfWork.AddAsync(model, cancellationToken);
        
        await publishEndpoint.Publish(new CurrencyCreatedEvent
        {
            Currency = model.Adapt<global::Contracts.Models.Currency.Currency>()
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await mediator.Publish(new CurrencyCreatedNotification(model.Id), cancellationToken);
        return new CreateCurrencyResult(model.Id);
    }
}