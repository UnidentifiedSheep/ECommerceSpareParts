using Microsoft.EntityFrameworkCore;

namespace Main.Persistence.Context;

internal sealed class BulkProductContext(DbContextOptions<BulkProductContext> options) : DbContext(options)
{
	public DbSet<BulkProduct> Products => Set<BulkProduct>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var product = modelBuilder.Entity<BulkProduct>();
		product.ToTable("products", "public");
		product.HasKey(x => x.Id);
		product.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
		product.Property(x => x.PairId).HasColumnName("pair_id");
		product.Property(x => x.Sku).HasColumnName("sku");
		product.Property(x => x.NormalizedSku).HasColumnName("normalized_sku");
		product.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
		product.Property(x => x.Stock).HasColumnName("stock");
		product.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
		product.Property(x => x.PackingUnit).HasColumnName("packing_unit");
		product.Property(x => x.ProducerId).HasColumnName("producer_id");
		product.Property(x => x.Indicator).HasColumnName("indicator").HasMaxLength(24);
		product.Property(x => x.CategoryId).HasColumnName("category_id");
		product.Property(x => x.ProductGroupId).HasColumnName("product_group_id");
		product.Property(x => x.Popularity).HasColumnName("popularity").HasDefaultValue(1L);
		product.Property(x => x.RowVersion).HasColumnName("xmin").IsRowVersion();
		product.Property(x => x.CreatedAt).HasColumnName("created_at");
		product.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		product.Property(x => x.WhoCreated).HasColumnName("who_created");
		product.Property(x => x.WhoUpdated).HasColumnName("who_updated");
		product.HasIndex(x => new { x.NormalizedSku, x.ProducerId })
			.IsUnique()
			.HasDatabaseName("products_normalized_sku_producer_id_index");
	}
}
