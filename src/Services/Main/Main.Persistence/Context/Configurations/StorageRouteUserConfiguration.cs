using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class StorageRouteUserConfiguration : IEntityTypeConfiguration<StorageRoute>
{
	public void Configure(EntityTypeBuilder<StorageRoute> builder)
		=> builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(route => route.CarrierId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("storage_routes_users_id_fk");
}
