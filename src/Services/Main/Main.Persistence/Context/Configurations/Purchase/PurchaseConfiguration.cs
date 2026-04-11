using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Purchase;

public class PurchaseConfiguration : IEntityTypeConfiguration<Entities.Purchase>
{
    public void Configure(EntityTypeBuilder<Entities.Purchase> builder)
    {
        builder.ToTable("purchase");
        
        builder.HasKey(e => e.Id)
            .HasName("purchase_pk");


        builder.HasIndex(e => e.Comment, "purchase_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.CreatedUserId, "purchase_created_user_id_index");

        builder.HasIndex(e => e.CurrencyId, "purchase_currency_id_index");

        builder.HasIndex(e => e.PurchaseDatetime, "purchase_purchase_datetime_index");

        builder.HasIndex(e => e.State, "purchase_state_index");

        builder.HasIndex(e => e.Storage, "purchase_storage_index");

        builder.HasIndex(e => e.SupplierId, "purchase_supplier_id_index");

        builder.HasIndex(e => e.TransactionId, "purchase_transaction_id_index");

        builder.HasIndex(e => e.UpdatedUserId, "purchase_updated_user_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");
            
        builder.Property(e => e.CreatedUserId)
            .HasColumnName("created_user_id");
            
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
            
        builder.Property(e => e.PurchaseDatetime)
            .HasColumnName("purchase_datetime");
            
        builder.Property(e => e.State)
            .HasColumnName("state");
            
        builder.Property(e => e.Storage)
            .HasMaxLength(128)
            .HasColumnName("storage");
        
        builder.Property(e => e.SupplierId)
            .HasColumnName("supplier_id");
            
        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");
            
        builder.Property(e => e.UpdatedUserId)
            .HasColumnName("updated_user_id");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_users_id_fk");

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_currency_id_fk");

        builder.HasOne<Storage>()
            .WithMany(p => p.Purchases)
            .HasForeignKey(d => d.Storage)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_storages_name_fk");

        builder.HasOne(d => d.Supplier)
            .WithMany()
            .HasForeignKey(d => d.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_users_id_fk_2");

        builder.HasOne(d => d.Transaction)
            .WithMany()
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_transactions_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.UpdatedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_users_id_fk_3");
    }
}