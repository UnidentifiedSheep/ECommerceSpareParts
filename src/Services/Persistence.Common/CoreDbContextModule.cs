using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.Common.BaseTableConfigurations;
using Persistence.Interfaces;

namespace Persistence.Common;

public sealed class CoreDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");
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

		modelBuilder.ApplyConfiguration(new EntityHistoryConfiguration());
	}

	public void PostConfigure(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyMetadataConfigurations();
		modelBuilder.AddFieldsForAuditableEntities();

		modelBuilder.AllDateTimesToUtc().AllEnumsToString();
	}
}
