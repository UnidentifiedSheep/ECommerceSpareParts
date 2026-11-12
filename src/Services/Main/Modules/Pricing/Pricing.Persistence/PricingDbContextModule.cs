using Domain.CommonEntities;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.Common.BaseTableConfigurations;
using Persistence.Interfaces;
using Pricing.Entities.Settings;

namespace Pricing.Persistence;

public sealed class PricingDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.AddOutboxMessageEntity();
		modelBuilder.AddOutboxStateEntity();
		modelBuilder.AddInboxStateEntity();

		modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
		modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
		modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");

		modelBuilder
			.ApplyConfigurationsFromAssembly(typeof(PricingDbContextModule).Assembly)
			.ApplyConfiguration(new SettingConfiguration())
			.ApplyJobConfigurations();

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<Setting>(nameof(Setting))
			.HasValue<PricingSetting>(PricingSetting.SettingName);

		modelBuilder.ApplyMetadataConfigurations();
		modelBuilder.AddFieldsForAuditableEntities();

		modelBuilder.AllDateTimesToUtc().AllEnumsToString();
	}
}
