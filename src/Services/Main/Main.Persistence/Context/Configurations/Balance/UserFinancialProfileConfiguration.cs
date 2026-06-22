using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Balance;

public class UserFinancialProfileConfiguration : IEntityTypeConfiguration<UserFinancialProfile>
{
    public void Configure(EntityTypeBuilder<UserFinancialProfile> builder)
    {
        builder.ToTable("user_financial_profile", "public");
        
        builder.HasKey(e => e.UserId)
            .HasName("user_financial_profile_pk");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();
        
        builder.Property(e => e.MinAllowedBalance)
            .HasColumnName("min_allowed_balance");
        
        builder.Property(e => e.WalletBalance)
            .HasColumnName("wallet_balance");

        builder.Property(e => e.SystemBalance)
            .HasColumnName("system_balance");
        
        builder.HasOne<Entities.User.User>()
            .WithOne(e => e.FinancialProfile)
            .HasForeignKey<UserFinancialProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_financial_profile_users_id_fk");
    }
}
