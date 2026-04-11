using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Sale;

public class SaleConfiguration : IEntityTypeConfiguration<Entities.Sale.Sale>
{
    public void Configure(EntityTypeBuilder<Entities.Sale.Sale> builder)
    {
        builder.ToTable("sale");
        
        builder.HasKey(e => e.Id).HasName("sale_pk");

        builder.HasIndex(e => e.BuyerId, "sale_buyer_id_index");

        builder.HasIndex(e => e.Comment, "sale_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.CreatedUserId, "sale_created_user_id_index");

        builder.HasIndex(e => e.CurrencyId, "sale_currency_id_index");

        builder.HasIndex(e => e.MainStorageName, "sale_main_storage_name_index");

        builder.HasIndex(e => e.SaleDatetime, "sale_sale_datetime_index");

        builder.HasIndex(e => e.State, "sale_state_index");

        builder.HasIndex(e => e.TransactionId, "sale_transaction_id_index");

        builder.HasIndex(e => e.UpdatedUserId, "sale_updated_user_id_index");
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.BuyerId)
            .HasColumnName("buyer_id");
        
        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");
        
        builder.Property(e => e.CreatedUserId)
            .HasColumnName("created_user_id");
        
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.MainStorageName)
            .HasMaxLength(128)
            .HasColumnName("main_storage_name");
        
        builder.Property(e => e.SaleDatetime)
            .HasColumnName("sale_datetime");
        
        builder.Property(e => e.State)
            .HasColumnName("state");
        
        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");
        
        builder.Property(e => e.UpdatedUserId)
            .HasColumnName("updated_user_id");
        
        builder.HasOne(d => d.Buyer)
            .WithMany()
            .HasForeignKey(d => d.BuyerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_users_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_users_id_fk_2");

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.MainStorageName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_storages_name_fk");

        builder.HasOne(d => d.Transaction)
            .WithMany()
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_transactions_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.UpdatedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_users_id_fk_3");
    }
}