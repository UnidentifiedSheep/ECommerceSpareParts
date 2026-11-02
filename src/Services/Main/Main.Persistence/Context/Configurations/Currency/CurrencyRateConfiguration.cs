using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Currency;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
	public void Configure(EntityTypeBuilder<CurrencyRate> builder)
	{
		builder.ToTable("currency_rates", "public");
		builder.HasKey(value => value.Id).HasName("currency_rates_pk");
		builder.Property(value => value.Id).HasColumnName("id");
		builder.Property(value => value.FromCurrencyId).HasColumnName("from_currency_id");
		builder.Property(value => value.ToCurrencyId).HasColumnName("to_currency_id");
		builder.Property(value => value.Rate).HasColumnName("rate");
		builder.HasIndex(value => new { value.FromCurrencyId, value.ToCurrencyId, value.CreatedAt, value.Id })
			.HasDatabaseName("currency_rates_pair_date_idx");
		builder.HasOne(value => value.FromCurrency).WithMany(currency => currency.RatesFrom)
			.HasForeignKey(value => value.FromCurrencyId).OnDelete(DeleteBehavior.Restrict);
		builder.HasOne(value => value.ToCurrency).WithMany(currency => currency.RatesTo)
			.HasForeignKey(value => value.ToCurrencyId).OnDelete(DeleteBehavior.Restrict);
	}
}
