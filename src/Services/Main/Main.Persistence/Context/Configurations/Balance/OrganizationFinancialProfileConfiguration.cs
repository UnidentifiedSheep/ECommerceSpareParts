using Main.Entities.Balance;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Balance;

public class OrganizationFinancialProfileConfiguration : IEntityTypeConfiguration<OrganizationFinancialProfile>
{
    public void Configure(EntityTypeBuilder<OrganizationFinancialProfile> builder)
    {
        builder.ToTable("organization_financial_profile", "public");

        builder.HasKey(e => e.OrganizationId)
            .HasName("organization_financial_profile_pk");

        builder.Property(e => e.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(e => e.MinAllowedBalance)
            .HasColumnName("min_allowed_balance");

        builder.HasOne<Entities.Organization.Organization>()
            .WithOne(e => e.FinancialProfile)
            .HasForeignKey<OrganizationFinancialProfile>(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("organization_financial_profile_organization_id_fk");
    }
}