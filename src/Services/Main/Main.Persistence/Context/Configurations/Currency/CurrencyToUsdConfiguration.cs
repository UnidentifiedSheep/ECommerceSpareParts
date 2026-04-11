using Main.Entities;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyToUsdConfiguration : IEntityTypeConfiguration<CurrencyToUsd>
{
    public void Configure(EntityTypeBuilder<CurrencyToUsd> builder)
    {
        builder.ToTable("currency_to_usd");
        
        builder.HasKey(e => e.CurrencyId)
            .HasName("currency_to_usd_pk");

        builder.Property(e => e.CurrencyId)
            .ValueGeneratedNever()
            .HasColumnName("currency_id");
        
        builder.Property(e => e.ToUsd).HasColumnName("to_usd");

        builder.HasOne<Entities.Currency.Currency>()
            .WithOne(p => p.CurrencyToUsd)
            .HasForeignKey<CurrencyToUsd>(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("currency_to_usd_currency_id_fk");
    }
}