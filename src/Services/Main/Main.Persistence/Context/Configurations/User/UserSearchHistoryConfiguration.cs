using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserSearchHistoryConfiguration : IEntityTypeConfiguration<UserSearchHistory>
{
    public void Configure(EntityTypeBuilder<UserSearchHistory> builder)
    {
        builder.ToTable("user_search_history");
        
        builder.HasKey(e => e.Id)
            .HasName("user_search_history_pk");

        builder.HasIndex(e => e.SearchDateTime)
            .HasDatabaseName("user_search_history_search_date_time_index");

        builder.HasIndex(e => e.SearchPlace)
            .HasDatabaseName("user_search_history_search_place_index");

        builder.HasIndex(e => new { e.UserId, e.SearchPlace })
            .HasDatabaseName("user_search_history_user_id_search_place_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Query)
            .HasColumnType("jsonb")
            .HasColumnName("query");
        
        builder.Property(e => e.SearchDateTime)
            .HasColumnName("search_date_time");
        
        builder.Property(e => e.SearchPlace)
            .HasColumnName("search_place");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_search_history_users_id_fk");
    }
}