using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("user_info", "auth");
        
        builder.HasKey(e => e.UserId)
            .HasName("user_info_pk");

        builder.HasIndex(e => e.Description)
            .HasDatabaseName("user_info_description_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("user_info_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.SearchColumn)
            .HasDatabaseName("user_info_search_column_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Surname)
            .HasDatabaseName("user_info_surname_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");
        
        builder.Property(e => e.Description)
            .HasColumnName("description");
        
        builder.Property(e => e.Name)
            .HasColumnName("name");
        
        builder.Property(e => e.SearchColumn)
            .HasColumnName("search_column");
        
        builder.Property(e => e.Surname)
            .HasColumnName("surname");

        builder.HasOne<Entities.User.User>()
            .WithOne(p => p.UserInfo)
            .HasForeignKey<UserInfo>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_info_users_id_fk");
    }
}