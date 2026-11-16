using Main.Entities.Organization.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class OrganizationMemberRelationsConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
	public void Configure(EntityTypeBuilder<OrganizationMember> builder)
	{
		builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_members_users_id_fk");
	}
}
