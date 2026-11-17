using Application.Common.Querying;
using Main.Entities.Organization.Enums.Organization;

namespace Main.Modules.Organization.Application.Configs;

public static class OrganizationSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<Entities.Organization.Organization.Organization, string>(x => x.Name)
			.Map<Entities.Organization.Organization.Organization, Guid>("id", x => x.Id)
			.Map<Entities.Organization.Organization.Organization, string>("name", x => x.Name)
			.Map<Entities.Organization.Organization.Organization, string>("systemName", x => x.SystemName)
			.Map<Entities.Organization.Organization.Organization, OrganizationType>("type", x => x.Type);
}
