using Main.Entities.Balance.Balance;
using Main.Entities.Currency;
using Main.Entities.Organization.Organization;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class TransactionRelationsConfiguration : IEntityTypeConfiguration<Transaction>
{
	public void Configure(EntityTypeBuilder<Transaction> builder)
	{
		builder
			.HasOne<Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_currency_id_fk");

		builder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(d => d.ReversedBy)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_users_id_fk_4");

		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(d => d.ReceiverId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_receiver_organization_id_fk");

		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(d => d.SenderId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_sender_organization_id_fk");
	}
}

public sealed class OrganizationBalanceRelationsConfiguration
	: IEntityTypeConfiguration<OrganizationBalance>
{
	public void Configure(EntityTypeBuilder<OrganizationBalance> builder)
	{
		builder
			.HasOne<Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("user_balances_currency_id_fk");

		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_balances_organizations_id_fk");
	}
}

public sealed class OrganizationFinancialProfileRelationsConfiguration
	: IEntityTypeConfiguration<OrganizationFinancialProfile>
{
	public void Configure(EntityTypeBuilder<OrganizationFinancialProfile> builder)
		=> builder
			.HasOne<Organization>()
			.WithOne()
			.HasForeignKey<OrganizationFinancialProfile>(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_financial_profile_organization_id_fk");
}
