using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Purchase.Persistence.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Entities.Purchase.Purchase.Purchase>
{
	public void Configure(EntityTypeBuilder<Entities.Purchase.Purchase.Purchase> builder)
	{
		builder.ToTable("purchase", "public");

		builder.HasKey(e => e.Id).HasName("purchase_pk");

		builder
			.HasIndex(e => e.Comment, "purchase_comment_index")
			.HasMethod("gin")
			.HasOperators("gin_trgm_ops");

		builder.HasIndex(e => e.CurrencyId, "purchase_currency_id_index");

		builder.HasIndex(e => e.PurchaseDatetime, "purchase_purchase_datetime_index");

		builder.HasIndex(e => e.State, "purchase_state_index");

		builder.HasIndex(e => e.Storage, "purchase_storage_index");

		builder.HasIndex(e => e.SupplierUserId, "purchase_supplier_user_id_index");

		builder.HasIndex(e => e.SupplierOrganizationId, "purchase_supplier_organization_id_index");

		builder.HasIndex(e => e.TransactionId, "purchase_transaction_id_index");

		builder
			.Property(e => e.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(e => e.Comment).HasMaxLength(256).HasColumnName("comment");

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.PurchaseDatetime).HasColumnName("purchase_datetime");

		builder.Property(e => e.State).HasColumnName("state");

		builder.Property(e => e.Storage).HasMaxLength(128).HasColumnName("storage");

		builder.Property(e => e.SupplierUserId).HasColumnName("supplier_user_id");

		builder.Property(e => e.SupplierOrganizationId).HasColumnName("supplier_organization_id");

		builder.Property(e => e.TransactionId).HasColumnName("transaction_id");

		builder
			.Navigation(e => e.Contents)
			.HasField("_contents")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}
