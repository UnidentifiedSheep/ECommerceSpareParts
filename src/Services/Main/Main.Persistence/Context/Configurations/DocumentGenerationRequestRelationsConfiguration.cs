using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class DocumentGenerationRequestRelationsConfiguration
	: IEntityTypeConfiguration<Entities.Documents.DocumentGenerationRequest>
{
	public void Configure(EntityTypeBuilder<Entities.Documents.DocumentGenerationRequest> builder)
		=> builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(request => request.RequesterId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("document_generation_requests_requester_fk");
}
