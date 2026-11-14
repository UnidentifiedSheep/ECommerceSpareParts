using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class StorageOwnerUserConfiguration : IEntityTypeConfiguration<StorageOwner>
{
	public void Configure(EntityTypeBuilder<StorageOwner> builder)
		=> builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(owner => owner.UserId)
			.HasConstraintName("storage_owners_users_id_fk");
}
