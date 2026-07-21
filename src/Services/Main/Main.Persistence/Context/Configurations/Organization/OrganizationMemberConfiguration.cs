using Main.Entities.Organization;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Organization;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("organization_members", "auth");

        builder.HasKey(x => new { x.OrganizationId, x.UserId })
            .HasName("organization_members_pk");

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.Role)
            .HasMaxLength(32)
            .HasColumnName("role");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("organization_members_user_id_index");

        builder.HasIndex(x => new { x.OrganizationId, x.Role })
            .HasDatabaseName("organization_members_owner_uindex")
            .IsUnique()
            .HasFilter($"role = '{OrganizationRole.Owner}'");

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("organization_members_organizations_id_fk");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("organization_members_users_id_fk");
    }
}
