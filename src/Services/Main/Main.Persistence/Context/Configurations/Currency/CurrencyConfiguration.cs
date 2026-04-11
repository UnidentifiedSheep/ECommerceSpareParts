using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyConfiguration : IEntityTypeConfiguration<Entities.Currency>
{
    public void Configure(EntityTypeBuilder<Entities.Currency> builder)
    {
        builder.ToTable("currency");
        
        builder.HasKey(e => e.Code)
            .HasName("currency_pk");

        builder.HasIndex(e => e.Code, "currency_code_uindex")
            .IsUnique();

        builder.HasIndex(e => e.CurrencySign, "currency_currency_sign_uindex")
            .IsUnique();

        builder.HasIndex(e => e.Name, "currency_name_uindex")
            .IsUnique();

        builder.HasIndex(e => e.ShortName, "currency_short_name_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Code)
            .HasMaxLength(26)
            .HasColumnName("code");
        
        builder.Property(e => e.CurrencySign)
            .HasMaxLength(3)
            .HasColumnName("currency_sign");
        
        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name");
        
        builder.Property(e => e.ShortName)
            .HasMaxLength(5)
            .HasColumnName("short_name");
    }
}