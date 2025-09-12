using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Currencies;
using Mapster;

namespace Application.Handlers.Currencies.CreateCurrency;

public record CreateCurrencyCommand(string ShortName, string Name, string CurrencySign, string Code)
    : ICommand<CreateCurrencyResult>;

public record CreateCurrencyResult(int Id);

public class CreateCurrencyHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCurrencyCommand, CreateCurrencyResult>
{
    public async Task<CreateCurrencyResult> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        await ValidateData(request.ShortName, request.Name, request.CurrencySign, request.Code, cancellationToken);
        var model = request.Adapt<Currency>();
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateCurrencyResult(model.Id);
    }

    private async Task ValidateData(string shortName, string name, string currencySign, string code,
        CancellationToken cancellationToken = default)
    {
        if (await currencyRepository.IsCurrencyCodeTaken(code, cancellationToken))
            throw new CurrencyCodeTakenException(code);
        if (await currencyRepository.IsCurrencyNameTaken(name, cancellationToken))
            throw new CurrencyNameTakenException(name);
        if (await currencyRepository.IsCurrencySignTaken(currencySign, cancellationToken))
            throw new CurrencySignTakenException(currencySign);
        if (await currencyRepository.IsCurrencyShortNameTaken(shortName, cancellationToken))
            throw new CurrencyShortNameTakenException(shortName);
    }
}