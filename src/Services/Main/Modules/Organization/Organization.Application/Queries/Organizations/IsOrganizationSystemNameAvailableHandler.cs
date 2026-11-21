using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Organization.Application.Queries.Organizations;

public record IsOrganizationSystemNameAvailableQuery(string SystemName)
	: IQuery<IsOrganizationSystemNameAvailableResult>;

public record IsOrganizationSystemNameAvailableResult(bool IsAvailable);

public class IsOrganizationSystemNameAvailableHandler(
	IReadRepository<Entities.Organization.Organization.Organization, Guid> repository)
	: IQueryHandler<IsOrganizationSystemNameAvailableQuery, IsOrganizationSystemNameAvailableResult>
{

	public async Task<IsOrganizationSystemNameAvailableResult> Handle(
		IsOrganizationSystemNameAvailableQuery request,
		CancellationToken cancellationToken)
	{
		var any = await repository.Query.AnyAsync(
			x => x.SystemName ==
				Entities.Organization.Organization.Organization.NormalizeSystemName(request.SystemName),
			cancellationToken);

		return new IsOrganizationSystemNameAvailableResult(!any);
	}
}
