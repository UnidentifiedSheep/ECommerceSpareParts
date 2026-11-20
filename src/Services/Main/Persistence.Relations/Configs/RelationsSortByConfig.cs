using Application.Common.Querying;
using Main.Entities.Balance.Balance;
using Main.Entities.Organization.Organization;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Relations.Configs;

internal static class RelationsSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.Map<Organization, decimal>(
				"approximateBalance",
				organization => (decimal?)EF.Property<OrganizationFinancialProfile>(
						organization,
						nameof(Organization.FinancialProfile))
					// ReSharper disable once ConstantNullCoalescingCondition
					.ApproximateBalance ?? 0m);
}
