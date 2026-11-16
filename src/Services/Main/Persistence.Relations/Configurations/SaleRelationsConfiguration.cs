using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class SaleRelationsConfiguration : IEntityTypeConfiguration<Entities.Sale.Sale>
{
	public void Configure(EntityTypeBuilder<Entities.Sale.Sale> builder)
	{
		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_user_id_fk");

		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_organizations_id_fk");

		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_currency_id_fk");

		builder
			.HasOne<Entities.Storage.Storage>()
			.WithMany()
			.HasForeignKey(d => d.StorageCode)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_storages_name_fk");

		builder
			.HasOne<Entities.Balance.Transaction>()
			.WithMany()
			.HasForeignKey(d => d.TransactionId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_transactions_id_fk");
	}
}
