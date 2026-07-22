using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Currencies;
using Main.Application.Projections;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Settings;
using Main.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;

public record GetOrganizationFinancialInfoQuery(Guid OrganizationId)
    : IQuery<GetOrganizationFinancialInfoResult>;

public record GetOrganizationFinancialInfoResult
{
    public required OrganizationFinancialProfileDto? FinancialProfile { get; init; }
    public required CurrencyDto BaseCurrency { get; init; }
    public required IEnumerable<OrganizationBalanceDto> Balances { get; init; }
}

public class GetOrganizationFinancialInfoHandler(
    ISettingsService settingsService,
    IReadRepository<Organization, Guid> readRepository,
    IReadRepository<Currency, int> currencyReadRepository,
    IBalanceService balanceService
) : IQueryHandler<GetOrganizationFinancialInfoQuery, GetOrganizationFinancialInfoResult>
{
    public async Task<GetOrganizationFinancialInfoResult> Handle(
        GetOrganizationFinancialInfoQuery request,
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
            .Where(x => x.Id == request.OrganizationId)
            .Include(x => x.FinancialProfile)
            .Include(x => x.Balances)
            .ThenInclude(x => x.Currency)
            .FirstAsync(cancellationToken);
        var netPosition = await balanceService.GetBalanceInBaseCurrencyAsync(
            organization.Id,
            cancellationToken);

        return new GetOrganizationFinancialInfoResult
        {
            FinancialProfile = organization.FinancialProfile is null
                ? null
                : BalanceProjections.ToOrganizationFinancialProfileDto(
                    organization.FinancialProfile,
                    netPosition),
            Balances = organization.Balances.Select(
                BalanceProjections.ToOrganizationBalanceDto.AsFunc()),
            BaseCurrency = baseCurrency
        };
    }
}
