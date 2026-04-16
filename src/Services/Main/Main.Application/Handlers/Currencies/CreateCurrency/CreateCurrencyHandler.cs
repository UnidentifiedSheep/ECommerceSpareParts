using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Currency;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Currency;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

[AutoSave]
[Transactional]
public record CreateCurrencyCommand(string ShortName, string Name, string CurrencySign, string Code)
    : ICommand<CreateCurrencyResult>;

public record CreateCurrencyResult(int Id);

public class CreateCurrencyHandler(
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publishEndpoint, 
    IPublisher publisher)
    : ICommandHandler<CreateCurrencyCommand, CreateCurrencyResult>
{
    public async Task<CreateCurrencyResult> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var model = Currency.Create(request.Name, request.ShortName, request.CurrencySign, request.Code);
        await unitOfWork.AddAsync(model, cancellationToken);

        await publishEndpoint.Publish(new CurrencyCreatedEvent
        {
            Currency = model.Adapt<Contracts.Models.Currency.Currency>()
        }, cancellationToken);
        
        await publisher.Publish(new CurrencyCreatedNotification(model.Id), cancellationToken);
        return new CreateCurrencyResult(model.Id);
    }
}