using Domain.CommonEntities;
using Main.Entities.Settings;
using Main.Entities.Settings.Supplier;
using Main.Persistence.DatabaseObjects;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Notification.Persistence;
using Persistence.Common;
using Persistence.Common.BaseTableConfigurations;
using Persistence.Interfaces;

namespace Main.Persistence;

public sealed class MainDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");
		modelBuilder.HasDbFunction(
				typeof(UserSearchDbFunctions)
					.GetMethod(nameof(UserSearchDbFunctions.ToUserSearchQuery))!)
			.HasName("user_search_query")
			.HasSchema("auth");

		modelBuilder.AddOutboxMessageEntity();
		modelBuilder.AddOutboxStateEntity();
		modelBuilder.AddInboxStateEntity();

		modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
		modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
		modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");

		modelBuilder
			.HasPostgresExtension("dblink")
			.HasPostgresExtension("pg_trgm")
			.HasPostgresExtension("pgcrypto");

		modelBuilder
			.ApplyConfigurationsFromAssembly(typeof(MainDbContextModule).Assembly)
			.ApplyConfiguration(new SettingConfiguration())
			.ApplyJobConfigurations()
			.AddNotifications();

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<Setting>(nameof(Setting))
			.HasValue<GlobalApplicationSetting>(GlobalApplicationSetting.SettingName)
			.HasValue<StorageContentSetting>(StorageContentSetting.SettingName)
			.HasValue<FavoritSupplierSetting>(FavoritSupplierSetting.SettingName)
			.HasValue<TmtrSupplierSetting>(TmtrSupplierSetting.SettingName);

		modelBuilder.ApplyConfiguration(new EntityHistoryConfiguration());
		modelBuilder.ApplyMetadataConfigurations();
		modelBuilder.AddFieldsForAuditableEntities();

		modelBuilder.AllDateTimesToUtc().AllEnumsToString();
	}
}
