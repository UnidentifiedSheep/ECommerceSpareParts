using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Interfaces.Services;
using Main.Application.Projections;
using Main.Entities.Balance;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;

public record GetOrganizationFinancialInfoQuery(Guid OrganizationId)
    : IQuery<GetOrganizationFinancialInfoResult>;

public record GetOrganizationFinancialInfoResult
{
    public required OrganizationFinancialProfileDto? FinancialProfile { get; init; }
    public required IEnumerable<OrganizationBalanceDto> Balances { get; init; }
}

public class GetOrganizationFinancialInfoHandler(
    IReadRepository<Organization, Guid> readRepository,
    IBalanceService balanceService,
    IProjectionProvider<OrganizationBalance, OrganizationBalanceDto> balanceProjection
) : IQueryHandler<GetOrganizationFinancialInfoQuery, GetOrganizationFinancialInfoResult>
{
    public async Task<GetOrganizationFinancialInfoResult> Handle(
        GetOrganizationFinancialInfoQuery request,
        CancellationToken cancellationToken)
    {
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
                : OrganizationFinancialProfileDtoFactory.Create(
                    organization.FinancialProfile,
                    netPosition),
            Balances = organization.Balances.Select(
                balanceProjection.Projection.AsFunc())
        };
    }
}
