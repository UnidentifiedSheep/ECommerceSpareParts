using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class OrganizationMemberRelationsConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
	public void Configure(EntityTypeBuilder<OrganizationMember> builder)
	{
		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("organization_members_users_id_fk");
	}
}
