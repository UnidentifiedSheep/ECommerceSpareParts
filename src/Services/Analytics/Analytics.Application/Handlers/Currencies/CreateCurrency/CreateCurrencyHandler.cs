using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Attributes;
using MediatR;

namespace Analytics.Application.Handlers.Currencies.CreateCurrency;

[Transactional]
public record CreateCurrencyCommand(int CurrencyId, decimal ToUsd) : ICommand;

public class CreateCurrencyHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCurrencyCommand>
{
    public async Task<Unit> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        if (await currencyRepository.GetCurrency(request.CurrencyId, false, cancellationToken) != null)
            return Unit.Value;

        await unitOfWork.AddAsync(new Currency
        {
            Id = request.CurrencyId,
            ToUsd = request.ToUsd
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}