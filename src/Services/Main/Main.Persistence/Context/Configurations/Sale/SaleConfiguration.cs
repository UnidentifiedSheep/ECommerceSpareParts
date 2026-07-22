using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Sale;

public class SaleConfiguration : IEntityTypeConfiguration<Entities.Sale.Sale>
{
    public void Configure(EntityTypeBuilder<Entities.Sale.Sale> builder)
    {
        builder.ToTable("sale", "public");

        builder.HasKey(e => e.Id).HasName("sale_pk");

        builder.HasIndex(e => e.UserId, "sale_user_id_index");
        builder.HasIndex(e => e.OrganizationId, "sale_organization_id_index");

        builder.HasIndex(e => e.Comment, "sale_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.CurrencyId, "sale_currency_id_index");

        builder.HasIndex(e => e.StorageName, "sale_storage_name_index");

        builder.HasIndex(e => e.SaleDatetime, "sale_sale_datetime_index");

        builder.HasIndex(e => e.State, "sale_state_index");

        builder.HasIndex(e => e.TransactionId, "sale_transaction_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");

        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.StorageName)
            .HasMaxLength(128)
            .HasColumnName("storage_name");

        builder.Property(e => e.SaleDatetime)
            .HasColumnName("sale_datetime");

        builder.Property(e => e.State)
            .HasColumnName("state");

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_user_id_fk");

        builder.HasOne(d => d.Organization)
            .WithMany()
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_organizations_id_fk");

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.StorageName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_storages_name_fk");

        builder.HasOne(d => d.Transaction)
            .WithMany()
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_transactions_id_fk");

        builder.Navigation(e => e.Contents)
            .HasField("_contents")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
