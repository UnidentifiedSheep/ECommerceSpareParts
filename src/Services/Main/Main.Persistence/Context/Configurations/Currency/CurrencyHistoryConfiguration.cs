using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyHistoryConfiguration : IEntityTypeConfiguration<CurrencyHistory>
{
    public void Configure(EntityTypeBuilder<CurrencyHistory> builder)
    {
        builder.ToTable("currency_history");
        
        builder.HasKey(e => e.Id)
            .HasName("currency_history_pk");

        builder.HasIndex(e => e.CurrencyId)
            .HasDatabaseName("IX_currency_history_currency_id");

        builder.HasIndex(e => e.Datetime)
            .HasDatabaseName("IX_currency_history_datetime");

        builder.HasIndex(e => e.NewValue)
            .HasDatabaseName("IX_currency_history_new_value");

        builder.HasIndex(e => e.PrevValue)
            .HasDatabaseName("IX_currency_history_prev_value");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.Datetime)
            .HasDefaultValueSql("now()")
            .HasColumnName("datetime");
        
        builder.Property(e => e.NewValue)
            .HasColumnName("new_value");
        
        builder.Property(e => e.PrevValue)
            .HasColumnName("prev_value");

        builder.HasOne<Entities.Currency>()
            .WithMany(p => p.CurrencyHistories)
            .HasForeignKey(d => d.CurrencyId)
            .HasConstraintName("currency_history_currency_id_fk");
    }
}