using Main.Entities.Sale.Sale;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class SaleContentDetailRelationsConfiguration : IEntityTypeConfiguration<SaleContentDetail>
{
	public void Configure(EntityTypeBuilder<SaleContentDetail> builder)
	{
		builder
			.HasOne<Main.Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_details_currency_id_fk");

		builder
			.HasOne<StorageContent>()
			.WithMany()
			.HasForeignKey(d => d.StorageContentId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_details_storage_content_id_fk");
	}
}
