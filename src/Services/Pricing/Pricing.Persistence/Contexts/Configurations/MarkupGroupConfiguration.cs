using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;

namespace Pricing.Persistence.Contexts.Configurations;

public class MarkupGroupConfiguration : IEntityTypeConfiguration<MarkupGroup>
{
	public void Configure(EntityTypeBuilder<MarkupGroup> builder)
	{
		builder.HasKey(e => e.Id).HasName("markup_group_pk");

		builder.ToTable("markup_group");

		builder.HasIndex(e => e.CurrencyId, "IX_markup_group_currency_id");

		builder.Property(e => e.Id).HasColumnName("id");

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.SourceMessageId).HasColumnName("source_message_id");

		builder.HasIndex(e => e.SourceMessageId, "markup_group_source_message_id_uindex").IsUnique();

		builder.Property(e => e.Name).HasColumnName("name");

		builder
			.Navigation(e => e.MarkupRanges)
			.HasField("_markupRanges")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}
