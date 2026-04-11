using Main.Entities;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageContentReservationConfiguration : IEntityTypeConfiguration<StorageContentReservation>
{
    public void Configure(EntityTypeBuilder<StorageContentReservation> builder)
    {
        builder.ToTable("storage_content_reservations");

        builder.HasKey(e => e.Id).HasName("storage_content_reservations_pk");

        builder.HasIndex(e => e.GivenCurrencyId, "IX_storage_content_reservations_given_currency_id");

        builder.HasIndex(e => e.WhoCreated, "IX_storage_content_reservations_who_created");

        builder.HasIndex(e => e.WhoUpdated, "IX_storage_content_reservations_who_updated");

        builder.HasIndex(e => new { e.ProductId, e.IsDone },
            "storage_content_reservations_product_id_is_done_index");

        builder.HasIndex(e => e.Comment, "storage_content_reservations_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.IsDone, "storage_content_reservations_is_done_index");

        builder.HasIndex(e => new { e.UserId, e.IsDone }, "storage_content_reservations_user_id_is_done_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Comment)
            .HasColumnName("comment");
        
        builder.Property(e => e.CurrentCount)
            .HasColumnName("current_count");
        
        builder.Property(e => e.GivenCurrencyId)
            .HasColumnName("given_currency_id");
        
        builder.Property(e => e.GivenPrice)
            .HasColumnName("given_price");
        
        builder.Property(e => e.InitialCount)
            .HasColumnName("initial_count");
        
        builder.Property(e => e.IsDone)
            .HasColumnName("is_done");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.WhoCreated)
            .HasColumnName("who_created");
        
        builder.Property(e => e.WhoUpdated)
            .HasColumnName("who_updated");

        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_reservations_products_id_fk");

        builder.HasOne<Entities.Currency.Currency>()
            .WithMany()
            .HasForeignKey(d => d.GivenCurrencyId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("storage_content_reservations_currency_id_fk");
        
        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("storage_content_reservations_users_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.WhoCreated)
            .HasConstraintName("storage_content_reservations_users_id_fk_3");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.WhoUpdated)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("storage_content_reservations_users_id_fk_2");
    }
}