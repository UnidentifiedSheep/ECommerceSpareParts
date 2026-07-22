using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Currencies;
using Main.Application.Projections;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Entities.Settings;
using Main.Application.Interfaces.Services;
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
    IReadRepository<Organization, Guid> readRepository,
    IReadRepository<Currency, int> currencyReadRepository,
    IBalanceService balanceService
) : IQueryHandler<GetUserFinancialInfoQuery, GetUserFinancialInfoResult>
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

        var organization = await readRepository.Query
            .Where(x => x.Id == request.UserId)
            .Include(x => x.FinancialProfile)
            .Include(x => x.Balances)
            .ThenInclude(x => x.Currency)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserNotFoundException(request.UserId);
        var netPosition = await balanceService.GetBalanceInBaseCurrencyAsync(
            organization.Id,
            cancellationToken);

        return new GetUserFinancialInfoResult
        {
            FinancialProfile = organization.FinancialProfile is null
                ? null
                : BalanceProjections.ToUserFinancialProfileDto(
                    organization.FinancialProfile,
                    netPosition),
            Balances = organization.Balances.Select(BalanceProjections.ToUserBalanceDto.AsFunc()),
            BaseCurrency = baseCurrency
        };
    }
}
