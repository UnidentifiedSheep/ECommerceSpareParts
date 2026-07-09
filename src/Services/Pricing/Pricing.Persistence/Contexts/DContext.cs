using System.Reflection;
using Domain.CommonEntities;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.BaseTableConfigurations;
using Persistence.Extensions;
using Pricing.Entities;
using Pricing.Entities.Settings;

namespace Pricing.Persistence.Contexts;

public partial class DContext : DbContext
{
    public DContext() { }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MarkupGroup> MarkupGroups { get; set; }

    public virtual DbSet<MarkupRange> MarkupRanges { get; set; }

    public virtual DbSet<PriceOffer> PriceOffers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();

        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
        modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
        modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(GetType())!)
            .ApplyConfiguration(new SettingConfiguration());

        modelBuilder.Entity<Setting>()
            .HasDiscriminator(e => e.Key)
            .HasValue<Setting>(nameof(Setting))
            .HasValue<PricingSetting>(PricingSetting.SettingName);

        modelBuilder.AddFieldsForAuditableEntities();

        modelBuilder.AllDateTimesToUtc()
            .AllEnumsToString();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}