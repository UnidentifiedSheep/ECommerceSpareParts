using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Purchase;

public class PurchaseConfiguration : IEntityTypeConfiguration<Entities.Purchase.Purchase>
{
    public void Configure(EntityTypeBuilder<Entities.Purchase.Purchase> builder)
    {
        builder.ToTable("purchase", "public");

        builder.HasKey(e => e.Id)
            .HasName("purchase_pk");


        builder.HasIndex(e => e.Comment, "purchase_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.CurrencyId, "purchase_currency_id_index");

        builder.HasIndex(e => e.PurchaseDatetime, "purchase_purchase_datetime_index");

        builder.HasIndex(e => e.State, "purchase_state_index");

        builder.HasIndex(e => e.Storage, "purchase_storage_index");

        builder.HasIndex(e => e.SupplierUserId, "purchase_supplier_user_id_index");

        builder.HasIndex(
            e => e.SupplierOrganizationId,
            "purchase_supplier_organization_id_index");

        builder.HasIndex(e => e.TransactionId, "purchase_transaction_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");


        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(e => e.PurchaseDatetime)
            .HasColumnName("purchase_datetime");

        builder.Property(e => e.State)
            .HasColumnName("state");

        builder.Property(e => e.Storage)
            .HasMaxLength(128)
            .HasColumnName("storage");

        builder.Property(e => e.SupplierUserId)
            .HasColumnName("supplier_user_id");

        builder.Property(e => e.SupplierOrganizationId)
            .HasColumnName("supplier_organization_id");

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.Storage)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_storages_name_fk");

        builder.HasOne(d => d.SupplierUser)
            .WithMany()
            .HasForeignKey(d => d.SupplierUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_supplier_user_id_fk");

        builder.HasOne(d => d.SupplierOrganization)
            .WithMany()
            .HasForeignKey(d => d.SupplierOrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_supplier_organization_id_fk");

        builder.HasOne(d => d.Transaction)
            .WithMany()
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_transactions_id_fk");

        builder.Navigation(e => e.Contents)
            .HasField("_contents")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
