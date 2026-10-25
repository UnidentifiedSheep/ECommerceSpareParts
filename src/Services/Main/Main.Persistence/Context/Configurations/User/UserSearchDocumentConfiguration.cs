using Main.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

internal sealed class UserSearchDocumentConfiguration : IEntityTypeConfiguration<UserSearchDocument>
{
	public void Configure(EntityTypeBuilder<UserSearchDocument> builder)
	{
		builder.ToTable("user_search", "auth");

		builder.HasKey(document => document.UserId)
			.HasName("user_search_pk");

		builder.Property(document => document.UserId)
			.HasColumnName("user_id").ValueGeneratedNever();

		builder.Property(document => document.SearchVector)
			.HasColumnName("search_vector")
			.HasColumnType("tsvector").IsRequired();

		builder.HasIndex(document => document.SearchVector)
			.HasMethod("gin")
			.HasDatabaseName("user_search_vector_index");

		builder.HasOne(document => document.User)
			.WithOne()
			.HasForeignKey<UserSearchDocument>(document => document.UserId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("user_search_users_id_fk");
	}
}
