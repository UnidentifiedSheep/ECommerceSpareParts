using Main.Entities.Balance.Balance;
using Main.Entities.Organization.Organization;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class PurchaseRelationsConfiguration : IEntityTypeConfiguration<Purchase>
{
	public void Configure(EntityTypeBuilder<Purchase> builder)
	{
		builder
			.HasOne<Main.Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_currency_id_fk");

		builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.SupplierUserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_supplier_user_id_fk");

		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(d => d.SupplierOrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_supplier_organization_id_fk");

		builder
			.HasOne<Transaction>()
			.WithMany()
			.HasForeignKey(d => d.TransactionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_transactions_id_fk");

		builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(d => d.Storage)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_storages_name_fk");
	}
}
