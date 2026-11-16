using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Balance.Balance;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Balance.Application.Handlers.Balance;

public sealed record GetApproximateBalancesByOrganizationIdsQuery : IQuery<GetApproximateBalancesByOrganizationIdsResult>
{
	public GetApproximateBalancesByOrganizationIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetApproximateBalancesByOrganizationIdsResult(IReadOnlyDictionary<Guid, decimal> Balances);

public sealed class GetApproximateBalancesByOrganizationIdsHandler(
	IReadRepository<OrganizationFinancialProfile, Guid> repository)
	: IQueryHandler<GetApproximateBalancesByOrganizationIdsQuery, GetApproximateBalancesByOrganizationIdsResult>
{
	public async Task<GetApproximateBalancesByOrganizationIdsResult> Handle(
		GetApproximateBalancesByOrganizationIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetApproximateBalancesByOrganizationIdsResult(new Dictionary<Guid, decimal>());

		var balances = await repository.Query
			.Where(profile => request.Ids.Contains(profile.OrganizationId))
			.ToDictionaryAsync(profile => profile.OrganizationId, profile => profile.ApproximateBalance, cancellationToken);

		return new GetApproximateBalancesByOrganizationIdsResult(balances);
	}
}
