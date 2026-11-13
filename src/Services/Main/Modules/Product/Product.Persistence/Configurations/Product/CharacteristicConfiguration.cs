using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class CharacteristicConfiguration : IEntityTypeConfiguration<Characteristic>
{
	public void Configure(EntityTypeBuilder<Characteristic> builder)
	{
		builder.ToTable("characteristics", "public");
		builder.HasKey(e => e.Id).HasName("characteristics_pk");
		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
		builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
		builder.Property(e => e.NormalizedName)
			.HasColumnName("normalized_name")
			.HasMaxLength(128)
			.IsRequired();
		builder.HasIndex(e => e.NormalizedName, "characteristics_normalized_name_uindex").IsUnique();
	}
}
