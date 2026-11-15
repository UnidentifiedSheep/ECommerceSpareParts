using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class TransactionRelationsConfiguration : IEntityTypeConfiguration<Entities.Balance.Transaction>
{
	public void Configure(EntityTypeBuilder<Entities.Balance.Transaction> builder)
	{
		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_currency_id_fk");

		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.ReversedBy)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_users_id_fk_4");

		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(d => d.ReceiverId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_receiver_organization_id_fk");

		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(d => d.SenderId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("transactions_sender_organization_id_fk");
	}
}

public sealed class OrganizationBalanceRelationsConfiguration
	: IEntityTypeConfiguration<Entities.Balance.OrganizationBalance>
{
	public void Configure(EntityTypeBuilder<Entities.Balance.OrganizationBalance> builder)
	{
		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("user_balances_currency_id_fk");

		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_balances_organizations_id_fk");
	}
}

public sealed class OrganizationFinancialProfileRelationsConfiguration
	: IEntityTypeConfiguration<Entities.Balance.OrganizationFinancialProfile>
{
	public void Configure(EntityTypeBuilder<Entities.Balance.OrganizationFinancialProfile> builder)
		=> builder
			.HasOne<Entities.Organization.Organization>()
			.WithOne()
			.HasForeignKey<Entities.Balance.OrganizationFinancialProfile>(d => d.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_financial_profile_organization_id_fk");
}
