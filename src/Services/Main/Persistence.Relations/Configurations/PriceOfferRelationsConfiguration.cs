using Main.Entities.Currency;
using Main.Entities.Product.Product;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Offers;

namespace Persistence.Relations.Configurations;

public sealed class PriceOfferRelationsConfiguration : IEntityTypeConfiguration<PriceOffer>
{
	public void Configure(EntityTypeBuilder<PriceOffer> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(x => x.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("price_offer_product_id_fk");

		builder
			.HasOne<Currency>()
			.WithMany()
			.HasForeignKey(x => x.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("price_offer_currency_id_fk");

		builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(x => x.OfferForStorage)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("price_offer_offer_for_storage_fk");
	}
}
