using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Transaction;

public class TransactionConfiguration : IEntityTypeConfiguration<Entities.Transaction>
{
    public void Configure(EntityTypeBuilder<Entities.Transaction> builder)
    {
        builder.ToTable("transactions");
        
        builder.HasKey(e => e.Id)
            .HasName("transactions_pk");


        builder.HasIndex(e => e.CurrencyId, "IX_transactions_currency_id");

        builder.HasIndex(e => e.CreationDate, "transactions_creation_date_index");

        builder.HasIndex(e => e.DeletedBy, "transactions_deleted_by_index");

        builder.HasIndex(e => e.IsDeleted, "transactions_is_deleted_index");

        builder.HasIndex(e => e.ReceiverId, "transactions_receiver_id_index");

        builder.HasIndex(e => new { e.SenderId, e.ReceiverId }, "transactions_sender_id_receiver_id_index");

        builder.HasIndex(e => e.Status, "transactions_status_index");

        builder.HasIndex(e => new { e.TransactionDatetime, e.Id }, "transactions_transaction_datetime_id_index");


        builder.HasIndex(e => e.TransactionDatetime, "transactions_transaction_datetime_sender_id_receiver_id_idx")
            .IsDescending();

        builder.HasIndex(e => e.WhoMadeUserId, "transactions_who_made_user_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        
        builder.Property(e => e.CreationDate)
            .HasDefaultValueSql("now()")
            .HasColumnName("creation_date")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");
            
        builder.Property(e => e.DeletedBy)
            .HasColumnName("deleted_by");
        
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted");
        
        builder.Property(e => e.ReceiverBalanceAfterTransaction)
            .HasColumnName("receiver_balance_after_transaction");
            
        builder.Property(e => e.ReceiverId)
            .HasColumnName("receiver_id");
        
        builder.Property(e => e.SenderBalanceAfterTransaction)
            .HasColumnName("sender_balance_after_transaction");
            
        builder.Property(e => e.SenderId)
            .HasColumnName("sender_id");
        
        builder.Property(e => e.Status)
            .HasMaxLength(28)
            .HasColumnName("status");
        
        builder.Property(e => e.TransactionDatetime)
            .HasColumnName("transaction_datetime");
            
        builder.Property(e => e.TransactionSum)
            .HasColumnName("transaction_sum");
        
        builder.Property(e => e.WhoMadeUserId)
            .HasColumnName("who_made_user_id");

            
        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_currency_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.DeletedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_users_id_fk_4");

            builder.HasOne<Entities.User>()
                .WithMany()
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk_2");

            builder.HasOne<Entities.User>()
                .WithMany()
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk");

            builder.HasOne<Entities.User>()
                .WithMany()
                .HasForeignKey(d => d.WhoMadeUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk_3");

            builder.HasQueryFilter(x => !x.IsDeleted);
    }
}