using System.Reflection;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.BaseTableConfigurations;
using Persistence.Extensions;

namespace Analytics.Persistence.Context;

public partial class DContext : DbContext
{
    public DContext()
    {
    }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricCalculationJob> MetricCalculationJobs { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<PurchasesFact> PurchasesFacts { get; set; }

    public virtual DbSet<SaleContent> SaleContents { get; set; }

    public virtual DbSet<SaleContentDetail> SaleContentDetails { get; set; }

    public virtual DbSet<SalesFact> SalesFacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(GetType())!)
            .ApplyConfiguration(new SettingConfiguration());

        modelBuilder.AddFieldsForAuditableEntities();

        modelBuilder.AllDateTimesToUtc()
            .AllEnumsToString();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}