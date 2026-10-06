using Main.Entities.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Documents;

public sealed class DocumentGenerationRequestConfiguration
	: IEntityTypeConfiguration<DocumentGenerationRequest>
{
	public void Configure(EntityTypeBuilder<DocumentGenerationRequest> builder)
	{
		builder.ToTable("document_generation_requests", "public");

		builder.HasKey(request => request.RequestId).HasName("document_generation_requests_pk");

		builder.Property(request => request.RequestId)
			.HasColumnName("request_id")
			.ValueGeneratedNever();
		builder.Property(request => request.JobId).HasColumnName("job_id");
		builder.Property(request => request.DocumentSystemName)
			.HasColumnName("document_system_name")
			.HasMaxLength(128)
			.IsRequired();

		builder.Property(request => request.RequesterId).HasColumnName("requester_id");
		builder.Property(request => request.CreatedAt).HasColumnName("created_at_utc");
		builder.Property(request => request.BucketName)
			.HasColumnName("bucket_name")
			.HasMaxLength(255);
		builder.Property(request => request.StorageKey)
			.HasColumnName("storage_key")
			.HasMaxLength(1024);
		builder.Property(request => request.GeneratedAt).HasColumnName("generated_at_utc");
		builder.Property(request => request.ExpiresAt).HasColumnName("expires_at_utc");

		builder.HasIndex(
			request => new { request.RequesterId, CreatedAtUtc = request.CreatedAt },
			"document_generation_requests_requester_created_idx");
		builder.HasIndex(
			request => request.JobId,
			"document_generation_requests_job_id_uq")
			.IsUnique();
		builder.HasIndex(
			request => request.ExpiresAt,
			"document_generation_requests_expires_at_idx")
			.HasFilter("expires_at_utc IS NOT NULL");

		builder.HasOne(request => request.Job)
			.WithOne()
			.HasForeignKey<DocumentGenerationRequest>(request => request.JobId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("document_generation_requests_job_fk");

		builder.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(request => request.RequesterId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("document_generation_requests_requester_fk");
	}
}
