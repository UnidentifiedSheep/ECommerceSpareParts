using Application.Common.Querying;
using Main.Entities.Organization;
using Main.Enums.Organization;

namespace Main.Application.Configs;

public static class OrganizationSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<Organization, string>(x => x.Name)
			.Map<Organization, Guid>("id", x => x.Id)
			.Map<Organization, string>("name", x => x.Name)
			.Map<Organization, string>("systemName", x => x.SystemName)
			.Map<Organization, OrganizationType>("type", x => x.Type);
}
