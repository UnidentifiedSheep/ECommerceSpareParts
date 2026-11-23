using Main.Entities.Currency;
using Main.Entities.Product.Product;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Offers;

namespace Persistence.Relations.Configurations;

public sealed class PriceOfferHistoryRelationsConfiguration : IEntityTypeConfiguration<PriceOfferHistory>
{
	public void Configure(EntityTypeBuilder<PriceOfferHistory> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(x => x.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("price_offer_history_product_id_fk");

		builder
			.HasOne<Currency>()
			.WithMany()
			.HasForeignKey(x => x.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("price_offer_history_currency_id_fk");

		builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(x => x.ForStorage)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("price_offer_history_for_storage_fk");
	}
}
