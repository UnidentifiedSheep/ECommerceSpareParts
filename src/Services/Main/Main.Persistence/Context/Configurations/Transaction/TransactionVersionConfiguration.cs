using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Transaction;

public class TransactionVersionConfiguration : IEntityTypeConfiguration<TransactionVersion>
{
    public void Configure(EntityTypeBuilder<TransactionVersion> builder)
    {
        builder.ToTable("transaction_versions");
        
        builder.HasKey(e => e.Id)
            .HasName("transaction_versions_pk");

        builder.HasIndex(e => e.ReceiverId, "transaction_versions_receiver_id_index");

        builder.HasIndex(e => e.SenderId, "transaction_versions_sender_id _index");

        builder.HasIndex(e => new { e.TransactionId, e.Version },
            "transaction_versions_transaction_id_version_uindex")
            .IsUnique();

        builder.HasIndex(e => e.VersionCreatedDatetime, "transaction_versions_version_created_datetime_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.ReceiverId)
            .HasColumnName("receiver_id");
        
        builder.Property(e => e.SenderId)
            .HasColumnName("sender_id ");
            
        builder.Property(e => e.Status)
            .HasMaxLength(28)
            .HasColumnName("status");
        
        builder.Property(e => e.TransactionDatetime)
            .HasColumnName("transaction_datetime");
        
        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");
        
        builder.Property(e => e.TransactionSum)
            .HasColumnName("transaction_sum");
        
        builder.Property(e => e.Version)
            .HasColumnName("version");
        
            
        builder.Property(e => e.VersionCreatedDatetime)
            .HasDefaultValueSql("now()")
            .HasColumnName("version_created_datetime")
            .ValueGeneratedOnAdd();

        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transaction_versions_currency_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transaction_versions_users_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.SenderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transaction_versions_users_id_fk_2");

        builder.HasOne<Entities.Transaction>()
            .WithMany()
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("transaction_versions_transactions_id_fk");
    }
}