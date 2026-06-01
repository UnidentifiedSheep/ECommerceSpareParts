using Main.Entities.Mailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public class EmailOutBoxConfiguration : IEntityTypeConfiguration<EmailOutBox>
{
    public void Configure(EntityTypeBuilder<EmailOutBox> builder)
    {
        builder.ToTable("email_outbox", "public");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Body)
            .HasColumnName("body");
        
        builder.Property(e => e.Subject)
            .HasColumnName("subject");
        
        builder.Property(e => e.Status)
            .HasColumnName("status");
        
        builder.Property(e => e.SentAt)
            .HasColumnName("sent_at");
        
        builder.Property(x => x.To)
            .HasColumnName("to");
        
        builder.HasIndex(e => new { e.To, e.Status }, "to_status_email_outbox_idx");
        builder.HasIndex(e => e.Status, "status_email_outbox_idx");
    }
}