using Main.Entities.Balance.Balance;
using Main.Entities.Currency;
using Main.Entities.Organization.Organization;
using Main.Entities.Sale.Sale;
using Main.Entities.Storage.Storage;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class SaleRelationsConfiguration : IEntityTypeConfiguration<Sale>
{
	public void Configure(EntityTypeBuilder<Sale> builder)
	{
		builder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_user_id_fk");

		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_organizations_id_fk");

		builder
			.HasOne<Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_currency_id_fk");

		builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(d => d.StorageCode)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_storages_name_fk");

		builder
			.HasOne<Transaction>()
			.WithMany()
			.HasForeignKey(d => d.TransactionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_transactions_id_fk");
	}
}
