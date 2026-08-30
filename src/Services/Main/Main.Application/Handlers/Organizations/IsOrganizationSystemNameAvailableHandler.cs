using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations;

public record IsOrganizationSystemNameAvailableQuery(string SystemName)
	: IQuery<IsOrganizationSystemNameAvailableResult>;

public record IsOrganizationSystemNameAvailableResult(bool IsAvailable);

public class IsOrganizationSystemNameAvailableHandler(IReadRepository<Organization, Guid> repository)
	: IQueryHandler<IsOrganizationSystemNameAvailableQuery, IsOrganizationSystemNameAvailableResult>
{

	public async Task<IsOrganizationSystemNameAvailableResult> Handle(
		IsOrganizationSystemNameAvailableQuery request,
		CancellationToken cancellationToken)
	{
		var any = await repository.Query.AnyAsync(
			x => x.SystemName == Organization.NormalizeSystemName(request.SystemName),
			cancellationToken);

		return new IsOrganizationSystemNameAvailableResult(!any);
	}
}
