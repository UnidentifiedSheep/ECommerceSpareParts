using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("currency_rates", "public");

        builder.HasKey(e => new { e.FromCurrencyId, e.ToCurrencyId })
            .HasName("currency_to_usd_pk");

        builder.HasIndex(e => e.ToCurrencyId)
            .HasDatabaseName("currency_to_usd_index");

        builder.Property(e => e.FromCurrencyId)
            .ValueGeneratedNever()
            .HasColumnName("from_currency_id");

        builder.Property(e => e.ToCurrencyId)
            .ValueGeneratedNever()
            .HasColumnName("to_currency_id");

        builder.Property(e => e.Rate)
            .HasColumnName("rate");

        builder.HasOne(e => e.FromCurrency)
            .WithMany(c => c.RatesFrom)
            .HasForeignKey(e => e.FromCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToCurrency)
            .WithMany(c => c.RatesTo)
            .HasForeignKey(e => e.ToCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}