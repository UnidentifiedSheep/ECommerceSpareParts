using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class SaleContentDetailRelationsConfiguration : IEntityTypeConfiguration<Entities.Sale.SaleContentDetail>
{
	public void Configure(EntityTypeBuilder<Entities.Sale.SaleContentDetail> builder)
	{
		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_details_currency_id_fk");

		builder
			.HasOne<Entities.Storage.StorageContent>()
			.WithMany()
			.HasForeignKey(d => d.StorageContentId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_details_storage_content_id_fk");
	}
}
