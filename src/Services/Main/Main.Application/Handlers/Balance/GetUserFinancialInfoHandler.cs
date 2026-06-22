using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Users;
using Main.Application.Projections;
using Main.Entities.Balance;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Setting;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance;

public record GetUserFinancialInfoQuery(Guid UserId) : IQuery<GetUserFinancialInfoResult>;

public record GetUserFinancialInfoResult
{
    public required UserFinancialProfileDto? FinancialProfile { get; init; }
    public required CurrencyDto BaseCurrency { get; init; }
    public required IEnumerable<UserBalanceDto> Balances { get; init; }
}

public class GetUserFinancialInfoHandler(
    ISettingsService settingsService,
    IReadRepository<User, Guid> readRepository,
    IReadRepository<Currency, int> currencyReadRepository) : IQueryHandler<GetUserFinancialInfoQuery, GetUserFinancialInfoResult>
{
    public async Task<GetUserFinancialInfoResult> Handle(
        GetUserFinancialInfoQuery request, 
        CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data.BaseCurrencyId;
        
        var baseCurrency = await currencyReadRepository.Query
            .AsExpandable()
            .Select(CurrencyProjections.ToDto)
            .FirstAsync(x => x.Id == baseCurrencyId, cancellationToken);
        //we need to cache it.

        return await readRepository.Query
            .Where(x => x.Id == request.UserId)
            .AsExpandable()
            .Select(x => new GetUserFinancialInfoResult
            {
                FinancialProfile = x.FinancialProfile == null 
                    ? null 
                    : BalanceProjections.ToUserFinancialProfileDto.Invoke(x.FinancialProfile),
                Balances = x.Balances.Select(z => BalanceProjections.ToUserBalanceDto.Invoke(z)),
                BaseCurrency = baseCurrency
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserNotFoundException(request.UserId);
    }
}