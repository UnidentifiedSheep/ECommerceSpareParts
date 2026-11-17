using Main.Entities.Documents.Documents;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class DocumentGenerationRequestRelationsConfiguration
	: IEntityTypeConfiguration<DocumentGenerationRequest>
{
	public void Configure(EntityTypeBuilder<DocumentGenerationRequest> builder)
		=> builder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(request => request.RequesterId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("document_generation_requests_requester_fk");
}
