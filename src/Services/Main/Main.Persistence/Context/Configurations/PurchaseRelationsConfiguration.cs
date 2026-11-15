using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class PurchaseRelationsConfiguration : IEntityTypeConfiguration<Entities.Purchase.Purchase>
{
	public void Configure(EntityTypeBuilder<Entities.Purchase.Purchase> builder)
	{
		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_currency_id_fk");

		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.SupplierUserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_supplier_user_id_fk");

		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(d => d.SupplierOrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_supplier_organization_id_fk");

		builder
			.HasOne<Entities.Balance.Transaction>()
			.WithMany()
			.HasForeignKey(d => d.TransactionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_transactions_id_fk");

		builder
			.HasOne<Entities.Storage.Storage>()
			.WithMany()
			.HasForeignKey(d => d.Storage)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_storages_name_fk");
	}
}
