using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class ProductReservationConfiguration : IEntityTypeConfiguration<ProductReservation>
{
    public void Configure(EntityTypeBuilder<ProductReservation> builder)
    {
        builder.ToTable("storage_content_reservations", "public");

        builder.HasKey(e => e.Id).HasName("storage_content_reservations_pk");

        builder.HasIndex(e => e.ProposedCurrencyId, "IX_storage_content_reservations_proposed_currency_id");

        builder.HasIndex(
            e => new { e.ProductId, e.Status },
            "storage_content_reservations_product_id_status_index");

        builder.HasIndex(e => e.Comment, "storage_content_reservations_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Status, "storage_content_reservations_status_index");

        builder.HasIndex(
            e => new { e.OrganizationId, e.Status },
            "storage_content_reservations_organization_id_status_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.Property(e => e.CurrentCount)
            .HasColumnName("current_count");

        builder.Property(e => e.ProposedCurrencyId)
            .HasColumnName("proposed_currency_id");

        builder.Property(e => e.ProposedPrice)
            .HasColumnName("proposed_price");

        builder.Property(e => e.ReservedCount)
            .HasColumnName("reserved_count");

        builder.Property(e => e.Status)
            .HasColumnName("status");

        builder.Property(e => e.OrganizationId)
            .HasColumnName("organization_id");

        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_reservations_products_id_fk");

        builder.HasOne<Entities.Currency.Currency>()
            .WithMany()
            .HasForeignKey(d => d.ProposedCurrencyId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("storage_content_reservations_currency_id_fk");

        builder.HasOne<Entities.Organization.Organization>(e => e.Organization)
            .WithMany()
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_reservations_organization_id_fk");
    }
}
