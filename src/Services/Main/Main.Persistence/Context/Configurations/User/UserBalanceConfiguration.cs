using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserBalanceConfiguration : IEntityTypeConfiguration<UserBalance>
{
    public void Configure(EntityTypeBuilder<UserBalance> builder)
    {
        builder.ToTable("user_balances");
        
        builder.HasKey(e => e.Id)
            .HasName("user_balances_pk");

        builder.HasIndex(e => e.Balance)
            .HasDatabaseName("user_balances_balance_index");

        builder.HasIndex(e => e.CurrencyId)
            .HasDatabaseName("user_balances_currency_id_index");

        builder.HasIndex(e => new { e.CurrencyId, e.UserId })
            .HasDatabaseName("user_balances_currency_id_user_id_uindex")
            .IsUnique();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("user_balances_user_id_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Balance)
            .HasColumnName("balance");
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne<Entities.Currency.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_balances_currency_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_balances_users_id_fk");
    }
}