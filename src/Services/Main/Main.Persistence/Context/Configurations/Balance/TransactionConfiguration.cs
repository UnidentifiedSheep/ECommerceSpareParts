using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Transaction;

public class TransactionConfiguration : IEntityTypeConfiguration<Entities.Balance.Transaction>
{
    public void Configure(EntityTypeBuilder<Entities.Balance.Transaction> builder)
    {
        builder.ToTable("transactions");
        
        builder.HasKey(e => e.Id)
            .HasName("transactions_pk");


        builder.HasIndex(e => e.CurrencyId, "IX_transactions_currency_id");

        builder.HasIndex(e => e.ReversedBy, "transactions_deleted_by_index");

        builder.HasIndex(e => e.ReceiverId, "transactions_receiver_id_index");

        builder.HasIndex(e => new { e.SenderId, e.ReceiverId }, "transactions_sender_id_receiver_id_index");

        builder.HasIndex(e => e.Type, "transactions_type_index");

        builder.HasIndex(e => new { e.TransactionDatetime, e.Id }, "transactions_transaction_datetime_id_index");

        builder.HasIndex(e => e.TransactionDatetime, "transactions_transaction_datetime_sender_id_receiver_id_idx")
            .IsDescending();
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.ReversedAt)
            .HasColumnName("reversed_at");
            
        builder.Property(e => e.ReversedBy)
            .HasColumnName("reversed_by");
            
        builder.Property(e => e.ReceiverId)
            .HasColumnName("receiver_id");
            
        builder.Property(e => e.SenderId)
            .HasColumnName("sender_id");
        
        builder.Property(e => e.Type)
            .HasMaxLength(28)
            .HasColumnName("type");
        
        builder.Property(e => e.Status)
            .HasConversion<long>()
            .HasColumnName("status");
        
        builder.Property(e => e.TransactionDatetime)
            .HasColumnName("transaction_datetime");
            
        builder.Property(e => e.Amount)
            .HasColumnName("amount");
        
        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

            
        builder.HasOne<Entities.Currency.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_currency_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.ReversedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_users_id_fk_4");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_users_id_fk_2");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.SenderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transactions_users_id_fk");
    }
}