using Main.Entities.Balance.Balance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Balance.Persistence.Configurations.Balance;

public class OrganizationBalanceConfiguration : IEntityTypeConfiguration<OrganizationBalance>
{
	public void Configure(EntityTypeBuilder<OrganizationBalance> builder)
	{
		builder.ToTable("organization_balances", "public");

		builder
			.HasKey(e => new
			{
				UserId = e.OrganizationId, e.CurrencyId
			})
			.HasName("organization_balances_pk");

		builder.HasIndex(e => e.Balance).HasDatabaseName("organization_balances_balance_index");

		builder.HasIndex(e => e.CurrencyId).HasDatabaseName("organization_balances_currency_id_index");

		builder
			.HasIndex(e => new
			{
				e.CurrencyId, UserId = e.OrganizationId
			})
			.HasDatabaseName("organization_balances_currency_id_user_id_uindex")
			.IsUnique();

		builder.HasIndex(e => e.OrganizationId).HasDatabaseName("organization_balances_user_id_index");

		builder.Property(e => e.Balance).HasColumnName("balance");

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.OrganizationId).HasColumnName("organization_id");

		builder.Property(e => e.RowVersion).HasColumnName("xmin").IsRowVersion();
	}
}
