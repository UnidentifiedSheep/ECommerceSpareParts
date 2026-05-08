using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyRateHistoryConfiguration : IEntityTypeConfiguration<CurrencyRateHistory>
{
    public void Configure(EntityTypeBuilder<CurrencyRateHistory> builder)
    {
        builder.ToTable("currency_rate_history", "public");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId });

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.FromCurrencyId)
            .HasColumnName("from_currency_id");

        builder.Property(e => e.ToCurrencyId)
            .HasColumnName("to_currency_id");

        builder.Property(e => e.PrevRate)
            .HasColumnName("prev_rate");

        builder.Property(e => e.NewRate)
            .HasColumnName("new_rate");

        builder.HasOne(e => e.CurrencyRate)
            .WithMany(r => r.History)
            .HasForeignKey(e => new { e.FromCurrencyId, e.ToCurrencyId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}