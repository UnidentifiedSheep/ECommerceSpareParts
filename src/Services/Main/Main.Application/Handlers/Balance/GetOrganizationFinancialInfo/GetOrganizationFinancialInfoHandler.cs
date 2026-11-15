using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Balances;
using Main.Application.Interfaces.Services;
using Main.Application.Projections;
using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;

public record GetOrganizationFinancialInfoQuery(Guid OrganizationId)
	: IQuery<GetOrganizationFinancialInfoResult>;

public record GetOrganizationFinancialInfoResult
{
	public required OrganizationFinancialProfileDto? FinancialProfile { get; init; }

	public required IReadOnlyCollection<OrganizationBalanceDto> Balances { get; init; }
}

public class GetOrganizationFinancialInfoHandler(
	IReadRepository<OrganizationFinancialProfile, Guid> profileRepository,
	IReadRepository<OrganizationBalance, UserBalanceKey> balanceRepository,
	IBalanceService balanceService,
	IProjectionProvider<OrganizationBalance, OrganizationBalanceDto> balanceProjection)
	: IQueryHandler<GetOrganizationFinancialInfoQuery, GetOrganizationFinancialInfoResult>
{
	public async Task<GetOrganizationFinancialInfoResult> Handle(
		GetOrganizationFinancialInfoQuery request,
		CancellationToken cancellationToken)
	{
		var profile = await profileRepository
			.Query
			.FirstOrDefaultAsync(x => x.OrganizationId == request.OrganizationId, cancellationToken);
		var balances = await balanceRepository
			.Query
			.Where(x => x.OrganizationId == request.OrganizationId)
			.Include(x => x.Currency)
			.ToListAsync(cancellationToken);
		var netPosition = await balanceService.GetBalanceInBaseCurrencyAsync(
			request.OrganizationId,
			cancellationToken);

		return new GetOrganizationFinancialInfoResult
		{
			FinancialProfile = profile is null
				? null
				: OrganizationFinancialProfileDtoFactory.Create(profile, netPosition),
			Balances = balances.Select(balanceProjection.ProjectionFunc).ToArray()
		};
	}
}
