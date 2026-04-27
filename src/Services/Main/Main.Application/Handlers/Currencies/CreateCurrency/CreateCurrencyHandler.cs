using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Currency;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

[AutoSave]
[Transactional]
public record CreateCurrencyCommand(string ShortName, string Name, string CurrencySign, string Code)
    : ICommand<CreateCurrencyResult>;

public record CreateCurrencyResult(int Id);

public class CreateCurrencyHandler(
    IUnitOfWork unitOfWork, 
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<CreateCurrencyCommand, CreateCurrencyResult>
{
    public async Task<CreateCurrencyResult> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var model = Currency.Create(request.Name, request.ShortName, request.CurrencySign, request.Code);
        await unitOfWork.AddAsync(model, cancellationToken);

        integrationEventScope.Add(new CurrencyCreatedEvent
        {
            Currency = new Contracts.Models.Currency.Currency
            {
                Id = model.Id,
                Name = model.Name,
                CurrencySign = model.CurrencySign,
                Code = model.Code,
                ShortName = model.ShortName,
                ToUsdRate = model.CurrencyToUsd?.ToUsd ?? 0
            }
        });
        
        return new CreateCurrencyResult(model.Id);
    }
}